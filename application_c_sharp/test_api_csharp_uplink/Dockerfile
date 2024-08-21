# Utilise l'image .NET SDK pour construire et exécuter les tests
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Installe ReportGenerator pour générer des rapports de couverture de code
# RUN dotnet tool install -g dotnet-reportgenerator-globaltool
# ENV PATH="$PATH:/root/.dotnet/tools"

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

# Add this to see coverage
# RUN dotnet add package coverlet.collector

RUN dotnet build

ENTRYPOINT ["dotnet", "test", "--filter", "Category!=ToDevelop"]

# Add this to see coverage
# ENTRYPOINT ["dotnet", "test", "--logger:trx", "--results-directory:/app/coverage-results", "--collect:\"XPlat Code Coverage\""]
# ENTRYPOINT ["reportgenerator", "-reports:/app/coverage-results/**/coverage.cobertura.xml", "-targetdir:/app/coverage-report", "-reporttypes:HtmlInline_AzurePipelines;TextSummary"]

