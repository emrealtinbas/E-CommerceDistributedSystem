# Phase 10 - Basket Service Skeleton With Redis-Backed Storage

## Goal

Create Basket Service with Clean Architecture and Redis-backed basket storage.

This phase is tracked in Jira as `SCRUM-10`.

## What Was Added

```text
services/basket/src
    Basket.Domain
    Basket.Application
    Basket.Infrastructure
    Basket.Api

services/basket/tests
    Basket.UnitTests
```

## API Endpoints

```text
GET    /api/baskets/{customerId}
POST   /api/baskets/{customerId}/items
DELETE /api/baskets/{customerId}/items/{productId}
DELETE /api/baskets/{customerId}
GET    /api/baskets/{customerId}/checkout
GET    /health
```

## Redis As Basket Store

Decision:

- Basket Service uses Redis as the basket state store.

Why this approach is chosen:

- Basket data is temporary customer session-like commerce state.
- Basket workloads are high read/write and latency-sensitive.
- Redis fits short-lived mutable data better than SQL for this phase.

Problem it solves:

- Avoids overloading SQL Server with frequent basket item changes.
- Keeps Basket Service independent from Catalog and Order databases.

Alternatives:

- SQL Server basket table.
- Client-side basket only.
- Order Service draft orders.

Trade-offs:

- Redis is fast but basket data must be treated as temporary.
- SQL Server gives stronger durability but adds more write overhead.
- Client-side baskets are simple but cannot be trusted for price or product validation.

Interview questions:

- Is Redis acceptable as a primary store for baskets?
- What happens if Redis data is lost?
- Why must checkout revalidate basket prices and stock?

## Clean Architecture Boundary

Dependency direction:

```text
Basket.Api -> Basket.Application -> Basket.Domain
Basket.Infrastructure -> Basket.Application + Basket.Domain
```

Why this approach is chosen:

- Domain rules stay independent of Redis and HTTP.
- Application use cases depend on repository abstraction.
- Infrastructure owns Redis implementation.

Problem it solves:

- Prevents Redis serialization details from leaking into domain or controllers.

Alternatives:

- Put Redis calls directly in controllers.
- Use a single Basket API project.
- Store basket as dynamic JSON without domain model.

Trade-offs:

- More files and projects.
- Clearer long-term maintainability and testability.

Interview questions:

- Why should controllers not call Redis directly?
- Where should basket business rules live?
- What belongs in Infrastructure?

## Domain Model

Entities:

```text
CustomerBasket
BasketItem
```

Domain rules:

- Customer id is required.
- Product id is required.
- Product name is required.
- Unit price cannot be negative.
- Currency must be a three-letter code.
- Quantity must be greater than zero.
- Adding the same product updates the existing item.

Why this approach is chosen:

- Basket looks simple, but invalid basket state can break checkout.
- Domain protects invariants even if the caller is not HTTP.

Problem it solves:

- Avoids invalid quantity, invalid currency, and duplicate product rows.

Alternatives:

- Validate only request DTOs.
- Use an anemic basket model.
- Represent basket as raw JSON only.

Trade-offs:

- Domain model adds structure.
- Raw JSON is flexible but unsafe.

Interview questions:

- What is the difference between request validation and domain invariant?
- Why update existing basket item instead of adding duplicates?
- Should basket store product name and price?

## Checkout Preparation Contract

Endpoint:

```text
GET /api/baskets/{customerId}/checkout
```

Purpose:

- Returns a basket snapshot that Order Service can later use as checkout input.

Important rule:

- This snapshot is not final truth for price or stock.
- Checkout must revalidate product price and availability with Catalog and Inventory boundaries in later phases.

Why this approach is chosen:

- Basket Service owns temporary customer selections.
- Order Service should create immutable order lines after validation.

Problem it solves:

- Separates temporary basket state from committed order state.

Alternatives:

- Order Service reads Redis basket directly.
- Basket Service creates orders directly.
- Client sends basket items directly to Order Service.

Trade-offs:

- Explicit checkout endpoint keeps boundaries clean.
- Later checkout workflow needs validation and saga coordination.

Interview questions:

- Why should Order Service not read Basket Redis directly?
- What data should be copied from basket to order?
- Why revalidate at checkout?

## Verification

Commands executed:

```powershell
dotnet build "E-CommerceDistributedSystem.sln"
dotnet test "services\basket\tests\Basket.UnitTests\Basket.UnitTests.csproj" --no-build
```

Expected result:

```text
Build succeeds with 0 warnings and 0 errors.
Basket unit tests pass.
```

## Recommended Phase 11

Phase 11 should add Basket API integration tests and Redis Testcontainers:

1. `WebApplicationFactory<Program>` for Basket API.
2. Redis Testcontainers test.
3. Basket endpoint integration coverage.
4. Basket checkout contract hardening.

## Approval Gate

Phase 10 is complete when Basket Service skeleton and Redis-backed storage are accepted.

Do not continue to Phase 11 until approval is received.
