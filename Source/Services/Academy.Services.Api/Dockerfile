# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base

ARG APP_UID=1000
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Services/Academy.Services.Api/Academy.Services.Api.csproj", "Services/Academy.Services.Api/"]
COPY ["Aspire/Academy.ServiceDefaults/Academy.ServiceDefaults.csproj", "Aspire/Academy.ServiceDefaults/"]
COPY ["Shared/Academy.Shared.Data/Academy.Shared.Data.csproj", "Shared/Academy.Shared.Data/"]
COPY ["Shared/Academy.Shared.Localisation/Academy.Shared.Localisation.csproj", "Shared/Academy.Shared.Localisation/"]
COPY ["Shared/Academy.Shared.Security.FusionAuth/ClassLibrary1/Academy.Shared.Security.FusionAuth.csproj", "Shared/Academy.Shared.Security.FusionAuth/ClassLibrary1/"]
COPY ["Shared/Academy.Shared.Security/Academy.Shared.Security/Academy.Shared.Security.csproj", "Shared/Academy.Shared.Security/Academy.Shared.Security/"]
COPY ["Shared/Academy.Shared.Storage.S3/Academy.Shared.Storage.S3.csproj", "Shared/Academy.Shared.Storage.S3/"]
COPY ["Shared/Academy.Shared.Storage/Academy.Shared.Storage.csproj", "Shared/Academy.Shared.Storage/"]
RUN dotnet restore "./Services/Academy.Services.Api/Academy.Services.Api.csproj"
COPY . .
WORKDIR "/src/Services/Academy.Services.Api"
RUN dotnet build "./Academy.Services.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build --no-restore

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Academy.Services.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-restore

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Metadata labels
LABEL maintainer="david@taitventures.com"
LABEL description="Academy Services API"

ENTRYPOINT ["dotnet", "Academy.Services.API.dll"]