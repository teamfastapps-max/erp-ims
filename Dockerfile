FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY erp-vims.sln ./
COPY VIMS.Web/VIMS.Web.csproj VIMS.Web/
COPY VIMS.DAL/VIMS.DAL.csproj VIMS.DAL/
COPY VIMS.Models/VIMS.Models.csproj VIMS.Models/
COPY VIMS.Helpers/VIMS.Helpers.csproj VIMS.Helpers/
COPY VIMS.Services/VIMS.Services.csproj VIMS.Services/
RUN dotnet restore

COPY . ./
RUN dotnet publish VIMS.Web/VIMS.Web.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish ./

EXPOSE 8080

ENTRYPOINT ["dotnet", "VIMS.Web.dll"]
