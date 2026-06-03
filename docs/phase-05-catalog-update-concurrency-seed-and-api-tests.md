# Phase 5 - Catalog Update, Concurrency, Seed Data, And API Tests

## Goal

Improve Catalog Service maturity by adding update/deactivate commands, optimistic concurrency handling, deterministic seed data, API integration test infrastructure, and a repository README.

## What Was Added

```text
Catalog.Application
    Common/Exceptions
        ConcurrencyConflictException.cs
        NotFoundException.cs
    Products/UpdateProduct
    Products/DeactivateProduct

Catalog.Infrastructure
    Persistence/SeedData
        CatalogSeedData.cs
    Persistence/UnitOfWork.cs
    Persistence/Migrations
        AddCatalogSeedData

Catalog.Api
    PUT    /api/products/{id}
    DELETE /api/products/{id}

Catalog.IntegrationTests
    Api
        CatalogApiFactory.cs
        ProductsApiTests.cs

README.md
```

## Optimistic Concurrency Decision

Catalog updates use SQL Server `rowversion`. API clients receive `RowVersion` as a base64 string and must send it back when updating or deactivating a product.

Why this approach is chosen:

- Catalog products can be edited by multiple administrators.
- Last-write-wins can silently overwrite another user's change.
- `rowversion` is a simple SQL Server-native concurrency token.

Problem it solves:

- Prevents lost updates during concurrent product modifications.

Alternatives:

- Last-write-wins.
- Manual integer version column.
- Pessimistic locks.
- Event sourcing.

Trade-offs:

- Clients must participate by sending the latest row version.
- Conflict handling adds API and UX complexity.
- Pessimistic locking avoids conflicts earlier but reduces throughput and can create blocking.

Interview questions:

- What is optimistic concurrency?
- What is a lost update?
- Why return HTTP 409 for concurrency conflicts?

## Update And Deactivate Commands

Commands:

```text
UpdateProductCommand
DeactivateProductCommand
```

Why this approach is chosen:

- Product changes are explicit use cases.
- Deactivation is preferred over hard delete because other services may reference historical product snapshots.

Problem it solves:

- Avoids deleting catalog records that could be needed for audits, orders, or reporting.

Alternatives:

- Hard delete products.
- Generic update endpoint that accepts partial dynamic fields.
- Store product status changes as events only.

Trade-offs:

- Soft delete/deactivation requires queries to be intentional about active/inactive data.
- Hard delete is simpler but risky in distributed systems.

Interview questions:

- Why avoid hard deletes in commerce systems?
- Where should update validation live?
- Should product price changes update existing orders?

## Unit Of Work Change

`CatalogDbContext` is no longer registered directly as `IUnitOfWork`. A dedicated `UnitOfWork` wraps `SaveChangesAsync` and translates EF Core concurrency exceptions into application-level exceptions.

Why this approach is chosen:

- Application layer should not depend on EF Core exception types.
- Infrastructure owns persistence-specific exception translation.

Problem it solves:

- Keeps Clean Architecture dependency direction intact.

Alternatives:

- Catch `DbUpdateConcurrencyException` directly in handlers.
- Let EF exceptions bubble to API.
- Use middleware to translate all exceptions.

Trade-offs:

- Adds one small infrastructure class.
- Keeps handlers cleaner and persistence-agnostic.

Interview questions:

- Is `DbContext` already a Unit of Work?
- Why translate infrastructure exceptions?
- Should application handlers reference EF Core?

## Seed Data

Seeded data:

```text
Categories:
    Books
    Electronics

Products:
    Wireless Headphones
    Domain-Driven Design
```

Why this approach is chosen:

- Local development and integration tests need deterministic baseline data.
- EF migrations keep seed data versioned with schema changes.

Problem it solves:

- Avoids manual database preparation after migration.

Alternatives:

- Runtime seeding on API startup.
- SQL scripts.
- Test-only seed setup.

Trade-offs:

- EF `HasData` is good for reference/demo data but not for large operational datasets.
- Runtime seeding is flexible but can be dangerous if not idempotent.

Interview questions:

- What data should be seeded through migrations?
- Why avoid startup seeding in production?
- How do you make seed data idempotent?

## API Integration Tests

`CatalogApiFactory` uses `WebApplicationFactory<Program>` and swaps the API database registration to a Testcontainers SQL Server connection.

Why this approach is chosen:

- Tests exercise the real ASP.NET Core pipeline.
- Tests validate routing, serialization, DI, EF Core, migrations, and seed data together.

Problem it solves:

- Catches problems that unit tests and repository-only tests cannot detect.

Alternatives:

- Controller unit tests.
- Postman/manual tests.
- Full end-to-end tests only.

Trade-offs:

- API integration tests are slower.
- They require Docker when enabled.
- They give higher confidence than isolated controller tests.

Interview questions:

- What is `WebApplicationFactory`?
- What is the difference between integration and end-to-end tests?
- Why test against the real database provider?

## Verification

Commands executed:

```powershell
dotnet ef migrations add AddCatalogSeedData --project "services\catalog\src\Catalog.Infrastructure\Catalog.Infrastructure.csproj" --startup-project "services\catalog\src\Catalog.Api\Catalog.Api.csproj" --output-dir "Persistence\Migrations"

dotnet build "E-CommerceDistributedSystem.sln"

dotnet test "services\catalog\tests\Catalog.IntegrationTests\Catalog.IntegrationTests.csproj" --no-build
```

Result:

```text
Build succeeded.
0 warnings.
0 errors.

Default tests completed.
2 Docker-backed tests skipped because RUN_DOCKER_INTEGRATION_TESTS was not enabled.
```

## Recommended Phase 6

Phase 6 should introduce Redis caching for Catalog reads:

1. Cache-aside strategy for product list and product by id.
2. Cache invalidation on update/deactivate.
3. Redis Docker Compose service.
4. Cache integration tests.

## Approval Gate

Phase 5 is complete when Catalog update/concurrency/seed/test changes are accepted.

Do not continue to Phase 6 until approval is received.
