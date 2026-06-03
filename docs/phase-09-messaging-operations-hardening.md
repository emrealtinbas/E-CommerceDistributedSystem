# Phase 9 - Messaging Operations Hardening

## Goal

Harden Catalog messaging operations after the reliable outbox publisher introduced in `SCRUM-8`.

This phase is tracked in Jira as `SCRUM-9`.

## What Was Added

```text
Catalog.Infrastructure
    Messaging
        RabbitMqHealthCheck.cs
        OutboxPublisherService.cs
    Persistence/Idempotency
        ProcessedMessage.cs
    Persistence/Configurations
        ProcessedMessageConfiguration.cs
    Persistence/Migrations
        AddOutboxClaimingAndProcessedMessages

Catalog.IntegrationTests
    Messaging
        RabbitMqContainerTests.cs
```

## SQL Row Claiming

The outbox publisher now claims rows before publishing them.

Claiming fields:

```text
LockId
LockedOnUtc
```

Claiming query uses SQL Server locking hints:

```sql
WITH PendingMessages AS
(
    SELECT TOP (@BatchSize) *
    FROM OutboxMessages WITH (UPDLOCK, READPAST, ROWLOCK)
    WHERE ProcessedOnUtc IS NULL
      AND DeadLetteredOnUtc IS NULL
      AND (LockedOnUtc IS NULL OR LockedOnUtc < @ExpiredBefore)
    ORDER BY OccurredOnUtc
)
UPDATE PendingMessages
SET LockId = @LockId,
    LockedOnUtc = @Now;
```

Why this approach is chosen:

- Multiple API replicas may run the same hosted outbox publisher.
- Without row claiming, two publishers can publish the same outbox message at the same time.
- `UPDLOCK` and `READPAST` allow concurrent publishers to skip locked rows and claim different work.

Problem it solves:

- Reduces duplicate publishes caused by concurrent publisher instances.

Alternatives:

- Run only one publisher instance.
- Use distributed locks with Redis.
- Move publisher to a single worker service.
- Use database queue libraries.

Trade-offs:

- SQL row claiming is simple and keeps coordination near the outbox table.
- It is SQL Server-specific because of lock hints.
- A separate worker service provides operational isolation but adds deployment complexity.

Interview questions:

- Why can multiple outbox publishers publish duplicates?
- What does `READPAST` do in SQL Server?
- What is the difference between row claiming and distributed locking?

## Claim Timeout

Configuration:

```json
"ClaimTimeoutSeconds": 60
```

Why this approach is chosen:

- A publisher can crash after claiming rows but before processing them.
- Expired claims allow another publisher to retry those rows.

Problem it solves:

- Prevents messages from being permanently stuck in a locked state.

Alternatives:

- No claim timeout.
- Heartbeat-based claim renewal.
- Manual operational unlock.

Trade-offs:

- Too short a timeout can increase duplicate publish risk during slow broker calls.
- Too long a timeout delays recovery after publisher crash.

Interview questions:

- Why do claimed rows need expiration?
- How do you choose lock timeout values?
- Can row claiming completely eliminate duplicates?

## RabbitMQ Health Check

`RabbitMqHealthCheck` was added to `/health`.

Why this approach is chosen:

- RabbitMQ is critical for event delivery.
- Operators need fast visibility into broker reachability.

Problem it solves:

- Detects broker connectivity problems without waiting for outbox backlog to grow.

Alternatives:

- Check only SQL Server health.
- Rely on logs from the publisher.
- Use external monitoring only.

Trade-offs:

- Health checks add connection overhead.
- A broker outage should not necessarily make all read APIs unavailable, but it should be visible.

Interview questions:

- Should message broker health be part of readiness checks?
- What is the difference between liveness and readiness?
- How should health checks behave when optional infrastructure is down?

## RabbitMQ Testcontainers Test

Docker-backed test added:

```text
RabbitMqContainerTests.Can_connect_and_declare_integration_exchange
```

Why this approach is chosen:

- Messaging code should be verified against a real broker.
- Declaring exchanges validates connection settings and RabbitMQ.Client compatibility.

Problem it solves:

- Catches broker/client API issues that mocks cannot detect.

Alternatives:

- Mock RabbitMQ client.
- Manual test through RabbitMQ Management UI.
- End-to-end test only after more services exist.

Trade-offs:

- Docker-backed tests are slower and require Docker.
- They provide much higher confidence than mocks for infrastructure behavior.

Interview questions:

- Why use Testcontainers for messaging tests?
- What should a broker integration test verify?
- Why are mocks risky for infrastructure libraries?

## Idempotent Consumer Foundation

Table added:

```text
ProcessedMessages
```

Composite key:

```text
MessageId + Consumer
```

Why this approach is chosen:

- Outbox plus RabbitMQ provides at-least-once delivery, not exactly-once processing.
- Consumers must remember processed message ids to ignore duplicates.

Problem it solves:

- Prevents future consumers from applying the same integration event more than once.

Alternatives:

- Rely on RabbitMQ exactly-once delivery.
- Store processed ids in Redis only.
- Make every operation naturally idempotent without a processed-message table.

Trade-offs:

- SQL storage is durable but adds writes per consumed message.
- Redis is fast but less suitable as the only durable idempotency record.
- Natural idempotency is ideal but not always possible for every side effect.

Interview questions:

- Why do consumers need idempotency if producers use outbox?
- What is at-least-once delivery?
- How would you design a processed-message table?

## Verification

Commands executed:

```powershell
dotnet ef migrations add AddOutboxClaimingAndProcessedMessages --project "services\catalog\src\Catalog.Infrastructure\Catalog.Infrastructure.csproj" --startup-project "services\catalog\src\Catalog.Api\Catalog.Api.csproj" --output-dir "Persistence\Migrations"

dotnet build "E-CommerceDistributedSystem.sln"

dotnet test "services\catalog\tests\Catalog.IntegrationTests\Catalog.IntegrationTests.csproj" --no-build

docker compose -f "deploy\docker-compose.yml" config
```

Expected result:

```text
Build succeeds with 0 warnings and 0 errors.
Default Docker-backed tests are skipped unless RUN_DOCKER_INTEGRATION_TESTS=true is set.
Docker Compose configuration is valid.
```

## Current Limitations

Not implemented yet:

- Outbox publisher metrics.
- Outbox backlog dashboard.
- Dead-letter replay tooling.
- Full consumer implementation.

## Recommended Phase 10

Phase 10 should start Basket Service:

1. Basket Service Clean Architecture skeleton.
2. Redis-backed basket storage.
3. Basket API endpoints.
4. Checkout preparation contract for Order Service.

## Approval Gate

Phase 9 is complete when messaging operations hardening is accepted.

Do not continue to Phase 10 until approval is received.
