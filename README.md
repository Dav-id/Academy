# Academy: Full Stack E-Learning Platform

Academy is a comprehensive, multi-tenant e-learning platform designed to deliver modular courses with rich video and text-based lessons, as well as robust assessment capabilities. This project demonstrates modern full stack development practices, secure architecture, and scalable design.

## Project Overview

Academy enables organisations to host and manage multi-module courses, each containing a mix of video and text lessons. The platform supports a variety of assessment types, including text responses, single/multiple choice, and true/false questions, at both lesson and course levels.

## Key Features

- **Role-Based Authentication**
	- Pluggable auth via interfaces for easy provider swaps
	- Built with [FusionAuth](https://fusionauth.io/) for robust user and role management

- **.NET Minimal API Backend**
	- Built with .NET Minimal APIs for simplicity and performance in a modular and easily extensible way
	- **EntityFramework Core** for data access and migrations
	- **PostgreSQL** as the primary database
	- **Redis** for distributed caching
	- **Aspire** for local development orchestration (logging, tracing, service management)
	- **S3-compatible (MinIO)** for asset storage (videos, documents), which can be easily swapped for alternative storage clients
	- **MS Test** for backend unit and integration testing

- **React Client (TypeScript)**
	- Modern, responsive UI built with React and TypeScript using Tailwind templates.
	- **React Router** for client-side navigation
    - **React Query** for data fetching and state management
	- **oidc-client-ts** for OpenID Connect authentication flows

- **Security & DevOps**
	- All secrets managed securely using [Vault](https://www.vaultproject.io/)
	- Localisation support on the API (translations), with frontend i18n planned
    - Docker for containerised development and deployment
    - Comprehensive role based authorisation at API level. 

- **Architecture**
	- Multi-tenant: supports multiple organizations with isolated data and configuration
	- Modular: easily extendable for new lesson types, assessment formats, and integrations such as adding additional Authentication providers, or replacing S3 (Minio) with file based storage. 

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/)
- [Node.js](https://nodejs.org/) (LTS)
- [Docker](https://www.docker.com/) (for Aspire, Postgres, Redis, MinIO, Vault)
- [FusionAuth](https://fusionauth.io/) (can be run via Docker)
- [Vault](https://www.vaultproject.io/)

MinIO, Vault, FusionAuth should be run via Docker outside of this orchestration. Later, I will add more instruction on this step. 

### Development Environment

The project uses [Aspire](https://devblogs.microsoft.com/dotnet/introducing-dotnet-aspire/) for orchestrating local development services (Postgres, Redis) with unified logging and tracing.

```sh
cd /Source/Aspire
# Start all services and the backend
dotnet watch run 
```

### Running the Client

```sh
cd client
npm install
npm run dev
```

## Project Structure

```
/.github                                                # GitHub workflows and CI/CD configuration

/Source/Aspire                                          # Local development orchestration & log viewer

/Source/Services/Academy.Services.Api                   # .NET Minimal API backend

/Source/Clients/Academy.Clients.Web                     # React + TypeScript frontend

/Source/Shared/Academy.Shared.Data                      # Shared data models & migrations (EF Core)
/Source/Shared/Academy.Shared.Localisation              # Localisation resources (e.g., translations) - Weblate can provide a web based interface to manage these
/Source/Shared/Academy.Shared.Security                  # Shared security models and interfaces
/Source/Shared/Academy.Shared.Security.FusionAuth       # Implementation security interfaces using FusionAuth APIs
/Source/Shared/Academy.Shared.Storage                   # Shared storage models and interfaces
/Source/Shared/Academy.Shared.Storage.S3                # Implementation of S3-compatible storage interfaces using MinIO APIs

/Source/Tests/Academy.Tests                             # Unit and integration tests using MSTest
```

## Security

- All sensitive configuration and secrets are managed via Vault.
- OIDC authentication with secure token storage.
- Role-based access control enforced at both API and UI levels.

## Localisation

- API supports multiple languages for all user-facing strings and content.
- Frontend localisation is planned for a future release.

## Multi-Tenancy

- Each organization (tenant) has isolated data, users, and configuration.
- Tenant context is enforced throughout the API and UI.

## Automation

- GitHub Actions: CI/CD pipelines are set up for automated testing and deployment. 

## License

AGPL-3.0 license 