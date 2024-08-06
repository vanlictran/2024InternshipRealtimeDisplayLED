# Utilise l'image .NET SDK pour construire et exécuter les tests
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copie les fichiers de solution et de projet et restaure les dépendances
COPY application_c_sharp.sln .
COPY api_csharp_uplink/api_csharp_uplink.csproj api_csharp_uplink/
COPY test_api_csharp_uplink/test_api_csharp_uplink.csproj test_api_csharp_uplink/
RUN dotnet restore

# Copie le reste des fichiers du projet et construit les tests
COPY api_csharp_uplink/. api_csharp_uplink/
COPY test_api_csharp_uplink/. test_api_csharp_uplink/
WORKDIR /app/test_api_csharp_uplink

RUN dotnet add package Moq
RUN dotnet add package FluentAssertions
RUN dotnet build

# Exécute les tests
ENTRYPOINT ["dotnet", "test", "--logger:trx"]
