# Phase 6 - Catalog Redis Cache-Aside Strategy

## Goal

Add distributed caching to Catalog Service read operations using Redis and the cache-aside pattern.

This phase adds read performance infrastructure without changing Catalog's source of truth. SQL Server remains the authoritative database.

## What Was Added

```text
Catalog.Application
    Abstractions/Caching
        IProductCache.cs

Catalog.Infrastructure
    Caching
        CacheKeys.cs
        RedisProductCache.cs

Catalog.Application query handlers
    GetProductByIdQueryHandler
    ListProductsQueryHandler

Catalog.Application command handlers
    CreateProductCommandHandler
    UpdateProductCommandHandler
    DeactivateProductCommandHandler

deploy
    docker-compose.yml
```

## Cache-Aside Decision

Catalog query handlers now follow this flow:

```text
Query arrives
    -> Try Redis cache
    -> If cache hit, return cached DTO
    -> If cache miss, query SQL Server
    -> Map entity to DTO
    -> Store DTO in Redis
    -> Return DTO
```

Why this approach is chosen:

- Catalog reads are usually much more frequent than writes.
- Product list and product-by-id are good cache candidates.
- Cache-aside keeps the database as the source of truth.

Problem it solves:

- Reduces repeated SQL Server load for hot product reads.
- Improves response latency for frequently requested catalog data.

Alternatives:

- No cache.
- Write-through cache.
- Read-through cache library.
- Dedicated read model or search index.
- CDN for public catalog responses.

Trade-offs:

- Cache-aside is simple and explicit but requires invalidation discipline.
- Write-through keeps cache warm but couples writes to cache availability.
- Search indexes are powerful for product discovery but add synchronization complexity.

Interview questions:

- What is cache-aside?
- When should you use distributed cache instead of in-memory cache?
- Should Redis be the source of truth?

## Cache Keys

Current keys:

```text
catalog:products:list
catalog:products:{productId}
```

Why this approach is chosen:

- Keys are service-scoped to avoid collisions with future services.
- Product list and product detail are cached separately because they have different access patterns.

Problem it solves:

- Prevents unrelated services or features from overwriting each other's cached data.

Alternatives:

- One generic key format for all services.
- Include API version in all keys.
- Include tenant id if multi-tenancy exists.

Trade-offs:

- Simple keys are easy to reason about now.
- Future multi-tenant or localized catalog reads will require richer key design.

Interview questions:

- How do you design Redis cache keys?
- What happens if cache keys collide?
- When should cache keys include version or tenant information?

## Expiration Policy

Current TTLs:

```text
Product detail: 10 minutes
Product list:   2 minutes
```

Why this approach is chosen:

- Product details are stable enough to cache longer.
- Product lists are broader and more likely to become stale after create/update/deactivate.

Problem it solves:

- Limits stale data lifetime even if invalidation fails.

Alternatives:

- No expiration and rely only on invalidation.
- Very short TTLs.
- Sliding expiration.

Trade-offs:

- Longer TTLs improve cache hit rate but increase stale-data risk.
- Shorter TTLs reduce staleness but provide less performance benefit.
- Sliding expiration is useful for session-like data, but catalog data is better controlled with absolute expiration.

Interview questions:

- What is TTL?
- What is the difference between absolute and sliding expiration?
- How do you choose cache expiration values?

## Invalidation Strategy

Invalidation happens after successful database commit:

```text
Create product
    -> Invalidate product list

Update product
    -> Invalidate product detail
    -> Invalidate product list

Deactivate product
    -> Invalidate product detail
    -> Invalidate product list
```

Why this approach is chosen:

- Cache should not be invalidated before the database write succeeds.
- Product list can change after create, update, or deactivate.
- Product detail must be removed after update or deactivate.

Problem it solves:

- Prevents serving stale product data after writes.

Alternatives:

- Update cache directly after writes.
- Publish invalidation events.
- Use very short TTL and skip explicit invalidation.

Trade-offs:

- Explicit invalidation is reliable inside one service but requires discipline.
- Event-based invalidation is better across services but requires messaging infrastructure.
- TTL-only is simple but accepts stale reads.

Interview questions:

- Why is cache invalidation hard?
- Should invalidation happen before or after database commit?
- What are stale reads?

## Redis Failure Handling

`RedisProductCache` treats Redis as non-critical infrastructure:

- Cache read failure logs a warning and behaves like a cache miss.
- Cache write failure logs a warning and continues.
- Cache invalidation failure logs a warning and continues.

Why this approach is chosen:

- SQL Server is the source of truth.
- Catalog API should remain available if Redis is temporarily unavailable.

Problem it solves:

- Prevents cache outages from becoming API outages.

Alternatives:

- Fail requests when Redis is down.
- Circuit breaker around Redis.
- Disable cache through configuration.

Trade-offs:

- Continuing without Redis improves availability but can increase SQL Server load.
- Failing fast makes cache issues visible but hurts user-facing reliability.
- Circuit breakers are useful and can be added later with resilience policies.

Interview questions:

- Should an application fail when cache is down?
- How do you monitor Redis failures?
- What is graceful degradation?

## Docker Compose Redis

Redis was added to:

```text
deploy/docker-compose.yml
```

Service:

```text
redis
```

Connection string:

```text
localhost:6379
```

Why this approach is chosen:

- Local development needs the same distributed cache dependency the service uses.
- Compose keeps SQL Server and Redis startup in one command.

Problem it solves:

- Avoids hidden local Redis installation requirements.

Alternatives:

- Install Redis locally.
- Use in-memory cache for development.
- Use cloud Redis during development.

Trade-offs:

- Docker Redis is realistic and disposable.
- In-memory cache would hide distributed-cache behavior across instances.

Interview questions:

- Why is in-memory cache not enough for multiple service instances?
- How does Redis help horizontal scaling?
- What Redis persistence mode is enabled here?

## Verification

Commands executed:

```powershell
dotnet build "E-CommerceDistributedSystem.sln"

docker compose -f "deploy\docker-compose.yml" config

dotnet test "services\catalog\tests\Catalog.IntegrationTests\Catalog.IntegrationTests.csproj" --no-build
```

Expected result:

```text
Build succeeds with 0 warnings and 0 errors.
Compose configuration is valid.
Docker-backed tests are skipped unless RUN_DOCKER_INTEGRATION_TESTS=true is set.
```

## Current Limitations

Not implemented yet:

- Redis health check endpoint integration.
- Cache hit/miss metrics.
- Resilience policy with circuit breaker.
- Event-driven cache invalidation across services.
- Product search cache or search index.

## Recommended Phase 7

Phase 7 should introduce RabbitMQ messaging foundation and Outbox preparation:

1. Messaging abstractions.
2. RabbitMQ Docker Compose service.
3. Integration event contracts.
4. Outbox table design for Catalog product changes.
5. Background outbox publisher skeleton.

## Approval Gate

Phase 6 is complete when Catalog Redis cache-aside behavior is accepted.

Do not continue to Phase 7 until approval is received.
