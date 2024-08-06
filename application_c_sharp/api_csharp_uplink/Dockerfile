FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY api_csharp_uplink.csproj ./
RUN dotnet add package InfluxDB.Client
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY certificate.pfx /app/certificate.pfx
COPY --from=build /app/out .

EXPOSE 8000
EXPOSE 8001

ENTRYPOINT ["dotnet", "api_csharp_uplink.dll"]