# Phase 3 - Catalog Service Skeleton With Clean Architecture

## Goal

Create the first real microservice skeleton: Catalog Service.

This phase establishes the implementation conventions that later services will follow:

1. Clean Architecture project separation.
2. Pragmatic DDD entities.
3. CQRS with MediatR.
4. FluentValidation pipeline behavior.
5. AutoMapper profile.
6. EF Core persistence boundary.
7. Repository and Unit of Work abstractions.
8. ASP.NET Core Web API with Swagger, Serilog, and health checks.

This phase intentionally does not implement migrations, Docker Compose, Redis, RabbitMQ, Outbox, or distributed workflows. Those belong to later phases.

## Project Structure

```text
services/catalog/src
    Catalog.Domain
        Entities
            Category.cs
            Product.cs
    Catalog.Application
        Abstractions/Persistence
            IProductRepository.cs
            IUnitOfWork.cs
        Common/Behaviors
            ValidationBehavior.cs
        Products
            CreateProduct
            GetProductById
            ListProducts
            Mapping
            Models
        DependencyInjection.cs
    Catalog.Infrastructure
        Persistence
            Configurations
            Repositories
            CatalogDbContext.cs
        DependencyInjection.cs
    Catalog.Api
        Controllers
            ProductsController.cs
        Program.cs
        appsettings.json
```

## Solution Projects

The following projects were added to `E-CommerceDistributedSystem.sln`:

```text
Catalog.Domain
Catalog.Application
Catalog.Infrastructure
Catalog.Api
```

## Dependency Direction

```text
Catalog.Api
    -> Catalog.Application
    -> Catalog.Infrastructure

Catalog.Infrastructure
    -> Catalog.Application
    -> Catalog.Domain

Catalog.Application
    -> Catalog.Domain

Catalog.Domain
    -> no project dependencies
```

Why this approach is chosen:

- Domain remains independent from frameworks, EF Core, HTTP, and messaging.
- Application owns use cases and abstractions.
- Infrastructure implements technical details.
- API is only the delivery mechanism.

Problem it solves:

- Prevents business rules from being coupled to controllers, database models, or external libraries.

Alternatives:

- Put everything in one Web API project.
- Use classic layered architecture where Domain depends on Infrastructure models.
- Use vertical slices without separate projects.

Trade-offs:

- More projects and files are created.
- Navigation is slightly heavier.
- Long-term maintainability and testability improve as the service grows.

Interview questions:

- Why should Domain not reference EF Core?
- What is the dependency rule in Clean Architecture?
- Is Clean Architecture overkill for every microservice?

## Domain Layer

Files:

```text
Catalog.Domain/Entities/Product.cs
Catalog.Domain/Entities/Category.cs
```

The `Product` entity owns core product invariants:

- Product name is required.
- Product price cannot be negative.
- Currency must be a three-letter ISO-like code.
- Product can be deactivated.
- Price changes happen through behavior, not public setters.

Why this approach is chosen:

- Even in a CRUD-looking service, important rules should live near the domain model.
- Private setters protect entity consistency.

Problem it solves:

- Avoids an anemic model where any layer can put an entity into an invalid state.

Alternatives:

- Use public setters and validate only in API requests.
- Use separate value objects for Money and ProductName immediately.
- Use full aggregate root base classes from the start.

Trade-offs:

- Current model is pragmatic and simple.
- More value objects would improve expressiveness but add complexity before the domain requires it.

Interview questions:

- What is the difference between validation and invariants?
- Why are private setters common in DDD entities?
- Should every property become a value object?

## Application Layer

Files:

```text
Catalog.Application/Products/CreateProduct
Catalog.Application/Products/GetProductById
Catalog.Application/Products/ListProducts
Catalog.Application/Common/Behaviors/ValidationBehavior.cs
Catalog.Application/Abstractions/Persistence
```

CQRS is implemented with MediatR:

- `CreateProductCommand` changes state.
- `GetProductByIdQuery` reads state.
- `ListProductsQuery` reads state.

Why this approach is chosen:

- Each use case has an explicit request and handler.
- Read and write intent is clear.
- Cross-cutting concerns such as validation can run through pipeline behaviors.

Problem it solves:

- Avoids large application services with many unrelated methods.

Alternatives:

- Controller directly calls repository.
- Use service classes such as `ProductService`.
- Full CQRS with separate read database.

Trade-offs:

- CQRS creates more files.
- For small features, handlers can feel verbose.
- The clarity becomes valuable as business workflows grow.

Interview questions:

- Does CQRS require separate databases?
- What belongs in a command handler?
- Why use pipeline behaviors in MediatR?

## Validation

File:

```text
Catalog.Application/Common/Behaviors/ValidationBehavior.cs
```

FluentValidation runs before handlers through a MediatR pipeline behavior.

Why this approach is chosen:

- Command/query validation is centralized.
- Handlers can assume request shape has already been checked.

Problem it solves:

- Prevents repeated validation calls inside every controller action.

Alternatives:

- ASP.NET Core model validation only.
- Manual validation inside handlers.
- Domain-only validation.

Trade-offs:

- FluentValidation validates request-level rules, not all domain invariants.
- Domain must still protect itself because not every caller is an HTTP request.

Interview questions:

- What is the difference between request validation and domain validation?
- Why validate before a command handler runs?
- Should FluentValidation replace domain rules?

## Mapping

File:

```text
Catalog.Application/Products/Mapping/ProductMappingProfile.cs
```

AutoMapper maps `Product` entities to `ProductDto`.

Why this approach is chosen:

- DTOs prevent leaking domain entities through the API.
- Mapping stays in Application because DTOs represent use case output.

Problem it solves:

- Avoids exposing EF/domain internals directly to clients.

Alternatives:

- Manual mapping.
- Source generators.
- Return domain entities directly.

Trade-offs:

- AutoMapper reduces repetitive mapping code but can hide mapping mistakes if used carelessly.
- Manual mapping is explicit but repetitive.

Interview questions:

- Why not return domain entities from controllers?
- What are the risks of AutoMapper?
- Where should DTO mapping live?

## Infrastructure Layer

Files:

```text
Catalog.Infrastructure/Persistence/CatalogDbContext.cs
Catalog.Infrastructure/Persistence/Configurations/ProductConfiguration.cs
Catalog.Infrastructure/Persistence/Configurations/CategoryConfiguration.cs
Catalog.Infrastructure/Persistence/Repositories/ProductRepository.cs
```

Infrastructure implements:

- EF Core `CatalogDbContext`.
- Entity configurations.
- `IProductRepository`.
- `IUnitOfWork` through the DbContext.
- SQL Server registration.
- EF Core health check.

Why this approach is chosen:

- Application depends on interfaces, not EF Core.
- Infrastructure owns persistence details.

Problem it solves:

- Keeps EF Core from leaking into handlers and domain entities beyond persistence configuration.

Alternatives:

- Use DbContext directly in handlers.
- Use generic repository.
- Use Dapper for queries and EF Core for commands.

Trade-offs:

- Repository adds abstraction but can be redundant if it only mirrors EF Core.
- Here it is used pragmatically because repository and unit of work are required enterprise patterns and define a clear persistence boundary.

Interview questions:

- Is Repository Pattern useful with EF Core?
- Is DbContext already a Unit of Work?
- Where should EF Core configurations live?

## Optimistic Concurrency Preparation

`Product.RowVersion` is configured as an EF Core row version.

Why this approach is chosen:

- Product updates such as price changes should detect concurrent writes.
- This prepares the Catalog Service for safe update commands in a later phase.

Problem it solves:

- Prevents lost updates when two administrators edit the same product concurrently.

Alternatives:

- Last write wins.
- Explicit version number column.
- Pessimistic database locks.

Trade-offs:

- Optimistic concurrency requires conflict handling in update use cases.
- Pessimistic locks can reduce concurrency and increase blocking.

Interview questions:

- What is optimistic concurrency?
- What is a lost update?
- How does SQL Server rowversion work with EF Core?

## API Layer

Files:

```text
Catalog.Api/Program.cs
Catalog.Api/Controllers/ProductsController.cs
```

Implemented endpoints:

```text
GET  /api/products
GET  /api/products/{id}
POST /api/products
GET  /health
```

API responsibilities:

- Register application and infrastructure dependencies.
- Enable Swagger in development.
- Configure Serilog.
- Expose controllers.
- Convert validation exceptions to HTTP 400.
- Expose health checks.

Why this approach is chosen:

- API stays thin and delegates business use cases to MediatR.

Problem it solves:

- Prevents controllers from becoming transaction scripts.

Alternatives:

- Minimal APIs.
- Controllers with injected repositories.
- Endpoint-per-feature libraries.

Trade-offs:

- Controllers are familiar and Swagger-friendly.
- Minimal APIs can be leaner, but controllers are still common in enterprise teams.

Interview questions:

- What should a controller contain?
- Why should validation errors return 400?
- What is the purpose of health checks?

## Verification

Command executed:

```powershell
dotnet build "E-CommerceDistributedSystem.sln"
```

Result:

```text
Build succeeded.
0 warnings.
0 errors.
```

## Important Limitations In This Phase

The Catalog Service is structurally valid but not production-complete yet.

Not implemented yet:

- EF Core migrations.
- Database creation scripts.
- Docker Compose SQL Server runtime.
- Product update/delete endpoints.
- Redis product read cache.
- Outbox integration events for product changes.
- Authentication/authorization.
- OpenTelemetry tracing.
- Integration tests.

These are intentionally deferred to preserve incremental delivery.

## Recommended Phase 4

Phase 4 should add Catalog persistence hardening:

1. EF Core migrations for `CatalogDb`.
2. SQL Server Docker Compose service.
3. Catalog API local run verification.
4. Seed data strategy.
5. Basic integration test using Testcontainers.

## Approval Gate

Phase 3 is complete when the Catalog Service skeleton is accepted.

Do not continue to Phase 4 until approval is received.
