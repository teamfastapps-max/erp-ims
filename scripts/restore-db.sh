#!/bin/bash
set -e

DB_REPO_DIR="/home/ubuntu/ims/erp_ims_db"
DB_FILE="VendorManagementDB.bak"
DB_CONTAINER="hms-db"
SA_PASSWORD="$1"

if [ -z "$SA_PASSWORD" ]; then
  echo "Usage: $0 <sa-password>"
  exit 1
fi

if [ ! -f "$DB_REPO_DIR/$DB_FILE" ]; then
  echo "Backup file not found at $DB_REPO_DIR/$DB_FILE"
  exit 1
fi

docker cp "$DB_REPO_DIR/$DB_FILE" "$DB_CONTAINER:/var/opt/mssql/backup/"

LOGICAL_DATA=$(docker exec "$DB_CONTAINER" /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "$SA_PASSWORD" -C \
  -h -1 \
  -Q "RESTORE FILELISTONLY FROM DISK = N'/var/opt/mssql/backup/$DB_FILE'" \
  | awk 'NR==1 {print $1; exit}')

LOGICAL_LOG=$(docker exec "$DB_CONTAINER" /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "$SA_PASSWORD" -C \
  -h -1 \
  -Q "RESTORE FILELISTONLY FROM DISK = N'/var/opt/mssql/backup/$DB_FILE'" \
  | awk 'NR==2 {print $1; exit}')

echo "Logical data name: $LOGICAL_DATA"
echo "Logical log name: $LOGICAL_LOG"

docker exec "$DB_CONTAINER" /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "$SA_PASSWORD" -C \
  -Q "RESTORE DATABASE [ims] FROM DISK = N'/var/opt/mssql/backup/$DB_FILE' WITH FILE = 1, \
       MOVE N'$LOGICAL_DATA' TO N'/var/opt/mssql/data/ims.mdf', \
       MOVE N'$LOGICAL_LOG' TO N'/var/opt/mssql/data/ims_log.ldf', \
       NOUNLOAD, REPLACE"

echo "ims database restored successfully"
