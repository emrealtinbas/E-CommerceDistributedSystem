# Phase 11 - Basket API Integration Tests With Redis Testcontainers

## Goal

Add Basket API integration test coverage against a real Redis container.

This phase is tracked in Jira as `SCRUM-11`.

## What Was Added

```text
services/basket/tests/Basket.IntegrationTests
    Api
        BasketApiFactory.cs
        BasketsApiTests.cs
    Infrastructure
        DockerFactAttribute.cs
```

## Integration Test Scope

Test covered:

```text
Can_add_get_remove_and_prepare_checkout_for_basket
```

Flow:

```text
Start Redis Testcontainer
Create Basket API test host
Override ConnectionStrings:Redis
POST basket item
GET basket
GET checkout snapshot
DELETE basket item
Assert persisted Redis-backed state
```

## Why WebApplicationFactory

Why this approach is chosen:

- It exercises the real ASP.NET Core request pipeline.
- It validates routing, controllers, MediatR, validation, DI, infrastructure registration, Redis repository, and serialization together.

Problem it solves:

- Unit tests cannot catch endpoint routing, dependency injection, serialization, or real Redis behavior problems.

Alternatives:

- Controller unit tests.
- Manual Swagger/Postman tests.
- Full end-to-end tests only.

Trade-offs:

- API integration tests are slower than unit tests.
- They require more infrastructure setup.
- They provide stronger confidence for service boundaries.

Interview questions:

- What is `WebApplicationFactory`?
- What is the difference between controller unit tests and API integration tests?
- Why test the real HTTP pipeline?

## Why Redis Testcontainers

Why this approach is chosen:

- Basket persistence behavior depends on Redis-specific behavior.
- A real Redis container is closer to production than an in-memory fake.
- Testcontainers gives isolated disposable infrastructure per test run.

Problem it solves:

- Avoids false confidence from mocks or in-memory substitutes.
- Avoids shared test Redis instances causing flaky tests.

Alternatives:

- Mock `IBasketRepository`.
- Use in-memory cache.
- Use a manually running local Redis.

Trade-offs:

- Docker-backed tests are slower and need Docker.
- They are more reliable than shared local infrastructure when CI is configured properly.

Interview questions:

- Why should Redis-backed code be tested with Redis?
- What is Testcontainers?
- How do you avoid flaky integration tests?

## Docker Opt-In

The Basket integration test uses:

```text
RUN_DOCKER_INTEGRATION_TESTS=true
```

Why this approach is chosen:

- Default test runs should not fail when Docker is unavailable.
- CI can choose when to run slower infrastructure tests.

Problem it solves:

- Keeps local and basic CI feedback fast and stable.

Trade-offs:

- Docker-backed tests can be skipped accidentally if CI never enables the flag.
- A dedicated nightly or main-branch pipeline should run them.

## Verification

Commands executed:

```powershell
dotnet build "E-CommerceDistributedSystem.sln"
dotnet test "services\basket\tests\Basket.UnitTests\Basket.UnitTests.csproj" --no-build
dotnet test "services\basket\tests\Basket.IntegrationTests\Basket.IntegrationTests.csproj" --no-build
dotnet test "services\catalog\tests\Catalog.IntegrationTests\Catalog.IntegrationTests.csproj" --no-build
```

Expected result:

```text
Build succeeds with 0 warnings and 0 errors.
Basket unit tests pass.
Docker-backed integration tests are skipped unless RUN_DOCKER_INTEGRATION_TESTS=true is set.
```

## Recommended Phase 12

Phase 12 should start Order Service foundation:

1. Order Service Clean Architecture skeleton.
2. Order aggregate and order lines.
3. Pending order creation from basket checkout snapshot.
4. SQL Server persistence and initial migration.
5. Outbox preparation for order workflow events.

## Approval Gate

Phase 11 is complete when Basket API integration test foundation is accepted.

Do not continue to Phase 12 until approval is received.
