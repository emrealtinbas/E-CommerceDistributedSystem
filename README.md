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
- Catalog Redis cache-aside strategy for product reads.
- Catalog integration events written through the Outbox Pattern.
- Reliable Catalog outbox publisher with RabbitMQ publisher confirms, retry tracking, and dead-letter exchange.
- Messaging operations hardening with SQL row claiming, RabbitMQ health check, RabbitMQ Testcontainers test, and idempotent consumer foundation.
- Basket Service Clean Architecture skeleton with Redis-backed basket storage and checkout preparation endpoint.
- Basket API integration tests with Redis Testcontainers and WebApplicationFactory.
- Docker Compose SQL Server foundation.
- Docker Compose Redis foundation.
- Docker Compose RabbitMQ foundation.
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
    basket
        src
            Basket.Api
            Basket.Application
            Basket.Domain
            Basket.Infrastructure
        tests
            Basket.IntegrationTests
            Basket.UnitTests
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

Start local infrastructure:

```powershell
docker compose -f "deploy\docker-compose.yml" up -d
```

This starts SQL Server, Redis, and RabbitMQ for the current Catalog Service phase.

RabbitMQ Management UI:

```text
http://localhost:15672
guest / guest
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
- `docs/phase-06-catalog-redis-cache-aside.md`
- `docs/phase-07-rabbitmq-messaging-and-outbox-preparation.md`
- `docs/phase-08-reliable-outbox-publisher.md`
- `docs/phase-09-messaging-operations-hardening.md`
- `docs/phase-10-basket-service-redis-backed-storage.md`
- `docs/phase-11-basket-api-integration-tests.md`
