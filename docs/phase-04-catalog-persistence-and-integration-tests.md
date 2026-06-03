# Phase 4 - Catalog Persistence And Integration Test Foundation

## Goal

Harden the Catalog Service persistence foundation by adding:

1. SQL Server Docker Compose infrastructure.
2. EF Core design-time support.
3. Initial Catalog database migration.
4. Testcontainers-based integration test foundation.

This phase focuses on database lifecycle and testability. It does not add Redis, RabbitMQ, Outbox, authentication, or business workflow logic.

## What Was Added

```text
deploy
    docker-compose.yml
    .env.example

services/catalog/src/Catalog.Infrastructure
    Persistence
        CatalogDbContextFactory.cs
        Migrations
            20260603132237_InitialCreate.cs
            20260603132237_InitialCreate.Designer.cs
            CatalogDbContextModelSnapshot.cs

services/catalog/tests/Catalog.IntegrationTests
    Infrastructure
        DockerFactAttribute.cs
    Persistence
        CatalogDatabaseFixture.cs
        CatalogDbContextTests.cs
    Catalog.IntegrationTests.csproj
```

## SQL Server Docker Compose

File:

```text
deploy/docker-compose.yml
```

Service:

```text
catalog-sqlserver
```

Connection string used by Catalog API:

```text
Server=localhost,1433;Database=CatalogDb;User Id=sa;Password=Your_password123;TrustServerCertificate=True
```

Why this approach is chosen:

- Every microservice must own its own database.
- Local development should be reproducible without manually installing SQL Server.
- Docker Compose is a pragmatic local orchestration tool for infrastructure dependencies.

Problem it solves:

- Avoids machine-specific SQL Server setup.
- Makes the database dependency explicit and versioned with the repository.

Alternatives:

- Install SQL Server directly on the developer machine.
- Use LocalDB.
- Use a shared development database.
- Use Kubernetes from the beginning.

Trade-offs:

- Docker Compose is simple for local development but is not a production orchestrator.
- LocalDB is easier on Windows but less representative of containerized production environments.
- Shared development databases create coupling and flaky tests.

Interview questions:

- Why use Docker Compose in local microservice development?
- Why should each service have its own database container or database instance?
- Is Docker Compose suitable for production?

## EF Core Design-Time Factory

File:

```text
Catalog.Infrastructure/Persistence/CatalogDbContextFactory.cs
```

Why this approach is chosen:

- EF Core CLI needs a reliable way to create `CatalogDbContext` at design time.
- The factory avoids depending on full API startup behavior during migration generation.

Problem it solves:

- Prevents migration commands from breaking when API startup gains dependencies such as messaging, cache, or external services.

Alternatives:

- Let EF Core create the DbContext through the startup project only.
- Put migrations in the API project.
- Use manual SQL scripts only.

Trade-offs:

- The factory contains a design-time connection string, so production secrets must never be placed there.
- Migrations stay close to Infrastructure, which is where EF Core belongs.

Interview questions:

- What is `IDesignTimeDbContextFactory`?
- Why can EF migrations fail without a design-time factory?
- Where should migrations live in Clean Architecture?

## Initial Migration

Migration:

```text
20260603132237_InitialCreate
```

Tables created:

```text
Categories
Products
```

Important database details:

- `Products.Price` uses `decimal(18,2)`.
- `Products.RowVersion` is configured as SQL Server `rowversion` for optimistic concurrency.
- `Products.Name` has an index.

Why this approach is chosen:

- Migrations version the database schema together with the application code.
- Schema changes become reviewable and repeatable.

Problem it solves:

- Avoids manual, undocumented database changes.

Alternatives:

- Manually maintained SQL scripts.
- Database-first EF Core scaffolding.
- Tools such as DbUp or Flyway.

Trade-offs:

- EF migrations are convenient for application-owned schemas.
- SQL-first migration tools can be better when DBAs own schema deployment.
- Generated migrations must still be reviewed.

Interview questions:

- What are EF Core migrations?
- Should migrations be committed to source control?
- How do you handle schema migrations in production?

## Integration Test Foundation

Project:

```text
services/catalog/tests/Catalog.IntegrationTests
```

Test infrastructure:

- `Testcontainers.MsSql` starts a real SQL Server container.
- `CatalogDbContextTests` applies EF migrations with `Database.MigrateAsync()`.
- The test persists and reads a real `Product` record.

Why this approach is chosen:

- EF Core behavior must be tested against the real provider, not only an in-memory substitute.
- SQL Server-specific features such as `rowversion`, precision, constraints, and migrations cannot be validated accurately with EF InMemory.

Problem it solves:

- Catches schema/provider issues before runtime.

Alternatives:

- EF Core InMemory provider.
- SQLite in-memory database.
- Shared SQL Server test database.
- Mock repositories only.

Trade-offs:

- Testcontainers tests are slower than unit tests.
- Docker must be available.
- Tests are much more realistic than provider fakes.

Interview questions:

- Why is EF Core InMemory not a good integration test replacement for SQL Server?
- What is Testcontainers?
- What should integration tests verify in a microservice?

## Docker Test Opt-In

Docker-backed tests are marked with:

```csharp
[DockerFact]
```

By default, they are skipped unless this environment variable is set:

```text
RUN_DOCKER_INTEGRATION_TESTS=true
```

Why this approach is chosen:

- The current environment has Docker CLI installed but Docker daemon is not available.
- Default `dotnet test` should not fail on machines or CI jobs that intentionally do not run Docker integration tests.
- Docker-backed tests can still be enabled explicitly in CI or local development.

Problem it solves:

- Prevents infrastructure-dependent tests from making the normal test command flaky.

Alternatives:

- Always run Docker tests.
- Never include Docker tests.
- Use a separate test solution.
- Use test categories and CI filtering only.

Trade-offs:

- Opt-in tests can be forgotten if CI does not enable them.
- Always-on Docker tests provide stronger safety but require reliable Docker availability.

Recommended CI rule:

- Pull requests should run build and unit tests always.
- Main branch or nightly pipelines should run Docker integration tests with `RUN_DOCKER_INTEGRATION_TESTS=true`.

Interview questions:

- Should integration tests run on every commit?
- How do you separate fast tests from infrastructure tests?
- How do you avoid flaky tests in CI?

## Commands

Start local SQL Server:

```powershell
docker compose -f "deploy\docker-compose.yml" up -d
```

Apply Catalog migrations manually:

```powershell
dotnet ef database update --project "services\catalog\src\Catalog.Infrastructure\Catalog.Infrastructure.csproj" --startup-project "services\catalog\src\Catalog.Api\Catalog.Api.csproj"
```

Run Catalog API:

```powershell
dotnet run --project "services\catalog\src\Catalog.Api\Catalog.Api.csproj"
```

Run default tests:

```powershell
dotnet test "services\catalog\tests\Catalog.IntegrationTests\Catalog.IntegrationTests.csproj"
```

Run Docker-backed integration tests:

```powershell
$env:RUN_DOCKER_INTEGRATION_TESTS = "true"
dotnet test "services\catalog\tests\Catalog.IntegrationTests\Catalog.IntegrationTests.csproj"
```

## Verification

Commands executed:

```powershell
dotnet ef migrations add InitialCreate --project "services\catalog\src\Catalog.Infrastructure\Catalog.Infrastructure.csproj" --startup-project "services\catalog\src\Catalog.Api\Catalog.Api.csproj" --output-dir "Persistence\Migrations"

docker compose -f "deploy\docker-compose.yml" config

dotnet build "E-CommerceDistributedSystem.sln"

dotnet test "services\catalog\tests\Catalog.IntegrationTests\Catalog.IntegrationTests.csproj" --no-build
```

Results:

```text
EF migration generated successfully.
Docker Compose configuration is valid.
Build succeeded with 0 warnings and 0 errors.
Default test run completed with Docker-backed test skipped.
```

Important environment note:

- Docker CLI exists in this environment.
- Docker daemon was not available during opt-in test execution, so the real containerized test could not be executed here.
- The test is ready to run when Docker is running and `RUN_DOCKER_INTEGRATION_TESTS=true` is set.

## Why Not Auto-Apply Migrations On API Startup

Decision:

- Do not automatically call `Database.Migrate()` from `Program.cs`.

Why this approach is chosen:

- Production schema changes should be controlled, observable, and reversible.
- App startup should not unexpectedly mutate production databases.

Problem it solves:

- Avoids multiple service instances racing to apply migrations during deployment.

Alternatives:

- Auto-apply migrations on startup.
- Run migrations from CI/CD pipeline.
- Run migrations from a dedicated migration job/container.

Trade-offs:

- Manual or pipeline migration requires one extra deployment step.
- Startup migration is convenient locally but risky in production.

Interview questions:

- Should applications run migrations automatically on startup?
- How do you deploy database changes safely?
- What happens if multiple replicas apply migrations at the same time?

## Current Limitations

Not implemented yet:

- Catalog seed data.
- Product update and delete commands.
- API-level integration tests with WebApplicationFactory.
- Redis caching.
- OpenTelemetry tracing.
- Outbox events for product changes.
- Authentication and authorization.

## Recommended Phase 5

Phase 5 should add Catalog read performance and API maturity:

1. Product update command with optimistic concurrency handling.
2. Product delete/deactivate behavior.
3. Seed data for local development.
4. API integration tests.
5. Optional Redis cache decision for catalog reads.

## Approval Gate

Phase 4 is complete when Catalog persistence and integration test foundation are accepted.

Do not continue to Phase 5 until approval is received.
