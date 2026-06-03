# E-Commerce Distributed System

Production-grade distributed e-commerce platform built incrementally with .NET 9, Clean Architecture, pragmatic DDD, CQRS, microservices, SQL Server, RabbitMQ, Redis, Docker, Serilog, OpenTelemetry, and health checks.

This repository is intentionally built phase by phase. It is not a tutorial CRUD sample or toy project. Each phase documents architectural decisions, alternatives, trade-offs, and interview questions.

## Current Status

Implemented so far:

- Repository and solution foundation.
- Catalog Service Clean Architecture skeleton.
- Catalog SQL Server persistence with EF Core migrations.
- Catalog create/list/get/update/deactivate API flow.
- Optimistic concurrency using SQL Server `rowversion`.
- Catalog seed data.
- Docker Compose SQL Server foundation.
- Testcontainers-based integration test foundation.

## Target Microservices

- Identity Service
- Catalog Service
- Basket Service
- Order Service
- Payment Service
- Inventory Service
- Notification Service

Each service owns its own database. Shared databases are intentionally forbidden.

## Repository Structure

```text
building-blocks
deploy
docs
services
    catalog
        src
            Catalog.Api
            Catalog.Application
            Catalog.Domain
            Catalog.Infrastructure
        tests
            Catalog.IntegrationTests
tests
```

## Prerequisites

- .NET SDK 9.0.312 or compatible .NET 9 SDK.
- Docker Desktop for SQL Server and Docker-backed integration tests.

## Build

```powershell
dotnet build "E-CommerceDistributedSystem.sln"
```

## Run Catalog SQL Server

```powershell
docker compose -f "deploy\docker-compose.yml" up -d
```

## Apply Catalog Migrations

```powershell
dotnet ef database update --project "services\catalog\src\Catalog.Infrastructure\Catalog.Infrastructure.csproj" --startup-project "services\catalog\src\Catalog.Api\Catalog.Api.csproj"
```

## Run Catalog API

```powershell
dotnet run --project "services\catalog\src\Catalog.Api\Catalog.Api.csproj"
```

Swagger is enabled in Development.

## Run Tests

Default test run skips Docker-backed tests:

```powershell
dotnet test "services\catalog\tests\Catalog.IntegrationTests\Catalog.IntegrationTests.csproj"
```

Run Docker-backed integration tests:

```powershell
$env:RUN_DOCKER_INTEGRATION_TESTS = "true"
dotnet test "services\catalog\tests\Catalog.IntegrationTests\Catalog.IntegrationTests.csproj"
```

## Architecture Documentation

- `docs/phase-01-solution-architecture-and-service-boundaries.md`
- `docs/phase-02-repository-and-solution-foundation.md`
- `docs/phase-03-catalog-service-skeleton.md`
- `docs/phase-04-catalog-persistence-and-integration-tests.md`
- `docs/phase-05-catalog-update-concurrency-seed-and-api-tests.md`
