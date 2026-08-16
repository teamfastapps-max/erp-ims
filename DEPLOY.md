# VIMS — CI/CD & Deployment Guide

## Architecture

```
GitHub ──push──► GitHub Actions ──build──► Docker image ──push──► ECR (vims-dev)
                   │                                                     │
                   └── SSH deploy ──► AWS instance (ip-172-31-34-29) ────┘
                                          │
                                          ├── docker-compose.yml (vims-app container :81)
                                          ├── ./erp_vims_db/ (git repo with .bak files)
                                          │
                                          └── hms-db container (existing SQL Server :1433)
```

- VIMS app runs in a Docker container on an existing AWS instance alongside the HMS app
- VIMS **does not** have its own database container — it connects to the existing `hms-db` SQL Server container via `host.docker.internal:1433`
- VIMS listens on **port 81** (HMS uses port 80)
- Database backup files live in a separate git repo (`erp_vims_db/`) on the server and are pulled + restored on every deploy

---

## Files & Their Roles

| File                                  | Purpose                                                    |
| ------------------------------------- | ---------------------------------------------------------- |
| `.github/workflows/docker-deploy.yml` | CI/CD pipeline: build, push to ECR, SSH deploy, DB restore |
| `Dockerfile`                          | Multi-stage .NET 8 build (SDK → runtime, no Chromium)      |
| `docker-compose.yml`                  | Production compose — vims-app only, no DB container        |
| `docker-compose.dev.yml`              | Local dev compose — vims-app + its own SQL Server          |
| `.env.example`                        | Template for environment variables                         |
| `.dockerignore`                       | Excludes bin/, obj/, .vs/, .git/, .env                     |
| `scripts/restore-db.sh`               | Manual DB restore script (not used by the workflow)        |

---

## CI/CD Pipeline (docker-deploy.yml)

### Trigger

- Pushes to `main` or `master` branch

### Steps

#### 1. Restore & Build (.NET)

```
dotnet restore
dotnet build --configuration Release --no-restore
```

#### 2. ECR Login & Auto-Create Repo

- Logs into AWS ECR (`304152263294.dkr.ecr.ap-south-1.amazonaws.com`)
- Checks if `vims-dev` repo exists; creates it if missing

#### 3. Build & Push Docker Image

- Builds Docker image tagged `vims-dev:latest`
- Pushes to `304152263294.dkr.ecr.ap-south-1.amazonaws.com/vims-dev:latest`

#### 4. SSH Deploy (appleboy/ssh-action)

Connects to the AWS instance and runs:

```bash
# Export AWS creds (from GitHub Secrets) so docker can pull from ECR
export AWS_ACCESS_KEY_ID="..."
export AWS_SECRET_ACCESS_KEY="..."
export AWS_DEFAULT_REGION=ap-south-1

cd /home/ubuntu/vims

# Authenticate with ECR
aws ecr get-login-password --region ap-south-1 \
  | docker login --username AWS --password-stdin 304152263294.dkr.ecr.ap-south-1.amazonaws.com

# Pull latest image & restart
docker compose pull
docker compose up -d
docker image prune -f
```

#### 5. DB Restore (Inline)

After the app container starts, the same SSH session runs the restore script:

1. **Pull latest `.bak`** from the `erp_vims_db` git repo
2. **Copy** the `.bak` file into the `hms-db` container at `/var/opt/mssql/backup/`
3. **FILELISTONLY** — prints the logical file names inside the `.bak`
4. **Restore** using fallback patterns:
   - `VendorManagementDB` / `VendorManagementDB_log`
   - `VIMS` / `VIMS_log`
   - `VendorManagement` / `VendorManagement_log`
   - `VIMS_Data` / `VIMS_Log`
   - `VIMS_data` / `VIMS_log`
5. **Verify** — queries `sys.databases` for `VIMS`

##### Password Handling

The SQL SA password is passed via `docker exec -e SQLCMDPASSWORD=` environment variable (not the `-P` flag) to avoid bash interpreting special characters (`$`, `!`, etc.). The password variable itself uses **single quotes** in the script:

```bash
SA_PASSWORD='${{ secrets.MSSQL_SA_PASSWORD }}'
```

---

## GitHub Secrets

| Secret                  | Description                           |
| ----------------------- | ------------------------------------- |
| `MSSQL_SA_PASSWORD`     | SA password for hms-db SQL Server     |
| `AWS_ACCESS_KEY_ID`     | AWS access key (ECR push permissions) |
| `AWS_SECRET_ACCESS_KEY` | AWS secret key                        |
| `VIMS_HOST`             | EC2 instance IP (`172.31.34.29`)      |
| `VIMS_USER`             | SSH user (`ubuntu`)                   |
| `VIMS_SSH_KEY`          | SSH private key for the EC2 instance  |

## GitHub Variables

| Variable      | Value  | Purpose                                                                                                                              |
| ------------- | ------ | ------------------------------------------------------------------------------------------------------------------------------------ |
| `DEPLOY_VIMS` | `true` | Gate for the deploy step (`if: ${{ vars.DEPLOY_VIMS == 'true' }}`). Set to `false` to skip deployment without removing the workflow. |

---

## Initial EC2 Setup (One-Time)

These commands were run once to prepare the server:

```bash
# SSH into the instance
ssh -i your-key.pem ubuntu@172.31.34.29

# Install Docker (if not already installed)
sudo apt update
sudo apt install -y docker.io docker-compose-v2
sudo usermod -aG docker $USER
# Log out and back in for group to take effect

# Install AWS CLI (if not already installed)
sudo apt install -y awscli

# Create VIMS deploy directory
mkdir -p /home/ubuntu/vims

# Clone VIMS repo
cd /home/ubuntu/vims && git clone https://github.com/teamfastapps-max/erp-vims.git .

# Clone DB backup repo
cd /home/ubuntu/vims && git clone <backup-repo-url> erp_vims_db

# Create .env file
cat > /home/ubuntu/vims/.env << 'EOF'
VIMS_IMAGE=304152263294.dkr.ecr.ap-south-1.amazonaws.com/vims-dev:latest
VIMS_HTTP_PORT=81
MSSQL_SA_PASSWORD=your-sa-password
EOF

# Verify HMS DB container is running (VIMS connects to this)
docker ps --filter name=hms-db

# First-time deploy (pull image and start)
export AWS_ACCESS_KEY_ID="..."
export AWS_SECRET_ACCESS_KEY="..."
export AWS_DEFAULT_REGION=ap-south-1
aws ecr get-login-password --region ap-south-1 \
  | docker login --username AWS --password-stdin 304152263294.dkr.ecr.ap-south-1.amazonaws.com
cd /home/ubuntu/vims
docker compose pull
docker compose up -d
```

---

## Environment Variables (.env)

```
VIMS_IMAGE=304152263294.dkr.ecr.ap-south-1.amazonaws.com/vims-dev:latest
VIMS_HTTP_PORT=81
MSSQL_SA_PASSWORD=your-sa-password
```

On the server, AWS credentials are **not** stored in `.env`. They are exported at deploy time from GitHub Secrets.

---

## Server Layout (EC2)

The EC2 instance only has deploy-time files — not the full project source (source lives in GitHub).

```
/home/ubuntu/
│
├── vims/                                   # VIMS deploy directory
│   ├── docker-compose.yml                  # Production compose (pulled from GitHub)
│   ├── .env                                # VIMS_IMAGE, VIMS_HTTP_PORT, MSSQL_SA_PASSWORD
│   └── erp_vims_db/                        # SEPARATE git repo — DB backup files only
│       ├── .git/
│       └── VendorManagementDB.bak          # ~3.2 MB (pulled + restored every deploy)

```

### Container Network

```
vims-app ──host.docker.internal:1433──► hms-db
```

Both apps reach the DB via `host.docker.internal:1433` (Docker Desktop host gateway pattern). The `extra_hosts` config in compose resolves `host.docker.internal` on Linux hosts.

### docker-compose.yml (on server)

```yaml
services:
  vims-app:
    image: 304152263294.dkr.ecr.ap-south-1.amazonaws.com/vims-dev:latest
    container_name: vims-app
    ports:
      - "81:8080"
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      ASPNETCORE_URLS: http://+:8080
      ConnectionStrings__DefaultConnection: Server=host.docker.internal,1433;Database=VIMS;User Id=sa;Password=${MSSQL_SA_PASSWORD};Encrypt=False;TrustServerCertificate=True;
    extra_hosts:
      - "host.docker.internal:host-gateway"
    restart: unless-stopped
```

### Useful Commands (on EC2)

```bash
# View logs
docker logs -f vims-app

# Restart app container
docker compose restart

# Full redeploy (pull + restart)
docker compose pull && docker compose up -d

# Check if VIMS DB exists
docker exec hms-db /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C \
  -Q "SELECT name, state_desc FROM sys.databases WHERE name = 'VIMS'"

# Manual DB restore (from server)
docker exec hms-db /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C \
  -Q "RESTORE DATABASE [VIMS] FROM DISK = N'/var/opt/mssql/backup/VendorManagementDB.bak' WITH FILE = 1, MOVE N'VendorManagementDB' TO N'/var/opt/mssql/data/VIMS.mdf', MOVE N'VendorManagementDB_log' TO N'/var/opt/mssql/data/VIMS_log.ldf', NOUNLOAD, REPLACE"

# Query VIMS tables
docker exec hms-db /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C \
  -Q "SELECT TOP 10 * FROM VIMS.dbo.Vendors_V"

# Prune old images
docker image prune -f

# Check running containers
docker ps --format "table {{.Names}}\t{{.Ports}}\t{{.Status}}"
```

---

## Local Development

```bash
# Clone & restore
git clone <repo>
cd erp-vims
dotnet restore

# Copy .env.example to .env and fill in values
cp .env.example .env

# Run with SQL Server in Docker
docker compose -f docker-compose.dev.yml up -d

# Or run directly (needs local SQL Server)
dotnet run --project VIMS.Web
```

---

## Key Decisions

- **No VIMS database container on server** — VIMS reuses the existing hms-db container
- **DB restore runs every deploy** — no conditional flag; simple and predictable
- **Stored procedures** — the existing VendorDAL calls stored procedures (`sp_Vendor_GetAll`, etc.). If the `.bak` doesn't contain them, those pages will error. The `DbTest` page uses raw SQL and works regardless.
- **Single environment** — only one branch-based deploy (main/master), no dev/prod split
- **`System.Data.SqlClient`** — kept for compatibility (no migration to `Microsoft.Data.SqlClient`)

---

## Testing

- **Dashboard**: `http://172.31.34.29:81/`
- **DB Test**: `http://172.31.34.29:81/DbTest` — reads/writes directly to `Vendors_V` table
- **Vendor module**: `http://172.31.34.29:81/Vendor/Vendor` (requires stored procedures in DB)
