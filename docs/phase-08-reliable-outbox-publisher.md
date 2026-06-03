# Phase 8 - Reliable Outbox Publisher, Publisher Confirms, Retry Tracking, And DLQ Strategy

## Goal

Complete the first reliable event publication path for Catalog Service by adding a background outbox publisher that reads pending outbox messages and publishes them to RabbitMQ.

This phase is tracked in Jira as `SCRUM-8`.

## What Was Added

```text
Catalog.Infrastructure
    Messaging
        OutboxPublisherService.cs
        RabbitMqOptions.cs
    Persistence/Outbox
        OutboxMessage.cs
    Persistence/Migrations
        AddOutboxDeadLetterTracking

Catalog.Api
    appsettings.json
```

## Publisher Flow

```text
OutboxPublisherService wakes up
    -> Opens RabbitMQ connection
    -> Opens channel with publisher confirmations enabled
    -> Declares integration exchange
    -> Declares dead-letter exchange
    -> Loads pending outbox messages from SQL Server
    -> Publishes message to RabbitMQ
    -> If broker confirms, marks message processed
    -> If publish fails, increments retry count and stores error
    -> If retry count exceeds limit, publishes to dead-letter exchange and marks dead-lettered
```

## Why Background Publisher

Why this approach is chosen:

- Product write requests should commit database state and return without waiting on broker availability.
- The outbox table creates a durable handoff between the request path and broker publishing.
- A background service can retry independently from user requests.

Problem it solves:

- Prevents RabbitMQ downtime from breaking product writes.
- Prevents losing events after SQL commit.

Alternatives:

- Publish directly inside command handlers.
- Use a separate worker service.
- Use MassTransit outbox.
- Use Debezium/change data capture.

Trade-offs:

- In-process publisher is simple but scales with API replicas and needs concurrency care later.
- A separate worker gives stronger operational isolation but adds deployment complexity.
- MassTransit is production-proven but hides useful learning details for this phase.

Interview questions:

- Why not publish directly from the request handler?
- What happens if RabbitMQ is down during an API request?
- Should outbox publishing run inside the API process or a separate worker?

## Publisher Confirms

RabbitMQ channel is created with publisher confirmations enabled.

Why this approach is chosen:

- A successful `BasicPublishAsync` with confirmations means the broker accepted the message.
- The outbox row is marked processed only after publish succeeds.

Problem it solves:

- Avoids marking messages as delivered when RabbitMQ rejected them.

Alternatives:

- Fire-and-forget publish.
- Transactions in RabbitMQ.
- Consumer-level acknowledgement only.

Trade-offs:

- Publisher confirms add latency compared with fire-and-forget publishing.
- The reliability gain is worth it for integration events.

Interview questions:

- What are RabbitMQ publisher confirms?
- Are consumer acknowledgements the same as publisher confirms?
- When should an outbox message be marked processed?

## Retry Tracking

Outbox messages track:

```text
RetryCount
Error
ProcessedOnUtc
DeadLetteredOnUtc
```

Why this approach is chosen:

- Failures need to be visible and recoverable.
- Retry count prevents infinite retry loops for poison messages.

Problem it solves:

- Avoids silently losing failed messages.
- Avoids retrying permanently broken payloads forever.

Alternatives:

- Retry forever.
- Delete failed messages.
- Move failed messages immediately to a dead-letter exchange.

Trade-offs:

- Retry tracking adds schema and operational monitoring requirements.
- Retry forever can overload systems.
- Immediate dead-lettering may give up too early on transient failures.

Interview questions:

- What is a poison message?
- How do you decide max retry count?
- What should be logged when publishing fails?

## Dead-Letter Strategy

When `RetryCount` reaches `MaxRetryCount`, the publisher sends the event payload to the configured dead-letter exchange:

```text
ecommerce.integration.dlx
```

The outbox row is then marked with `DeadLetteredOnUtc`.

Why this approach is chosen:

- Poison messages should be separated from normal publishing flow.
- Operators need a place to inspect failed integration events.

Problem it solves:

- Prevents one bad message from blocking all later messages forever.

Alternatives:

- Keep failed messages only in SQL Server.
- Delete failed messages after max retry.
- Use RabbitMQ queue-level DLX only.

Trade-offs:

- Publishing to a DLX keeps broker-side visibility but still needs monitoring.
- Keeping only SQL state is simpler but less aligned with broker operations.

Interview questions:

- What is a dead-letter queue?
- What is the difference between a DLQ and an outbox error column?
- How do you replay dead-lettered events safely?

## Idempotency Requirement

Outbox reduces message loss but does not guarantee exactly-once processing.

Consumers must be idempotent because:

- Publisher can crash after RabbitMQ accepts a message but before SQL row is marked processed.
- Retried messages can be published more than once.
- RabbitMQ provides at-least-once delivery patterns.

Recommended consumer rule:

- Store processed `MessageId` values and ignore duplicates.

Interview questions:

- Why can an outbox still publish duplicates?
- What is at-least-once delivery?
- How do consumers implement idempotency?

## Configuration

```json
"RabbitMQ": {
  "HostName": "localhost",
  "Port": 5672,
  "UserName": "guest",
  "Password": "guest",
  "ExchangeName": "ecommerce.integration",
  "DeadLetterExchangeName": "ecommerce.integration.dlx",
  "OutboxBatchSize": 20,
  "OutboxPollingIntervalSeconds": 5,
  "MaxRetryCount": 5
}
```

## Current Limitations

Known future hardening items:

- Prevent multiple API replicas from publishing the same outbox row concurrently.
- Add distributed lock or SQL row claiming.
- Add metrics for pending, processed, failed, and dead-lettered messages.
- Add integration test with RabbitMQ Testcontainers.
- Add replay tooling for dead-lettered messages.

These are intentionally deferred to keep this phase focused.

## Verification

Commands executed:

```powershell
dotnet ef migrations add AddOutboxDeadLetterTracking --project "services\catalog\src\Catalog.Infrastructure\Catalog.Infrastructure.csproj" --startup-project "services\catalog\src\Catalog.Api\Catalog.Api.csproj" --output-dir "Persistence\Migrations"

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

## Recommended Phase 9

Phase 9 should harden messaging operations:

1. SQL row claiming for outbox concurrency safety.
2. RabbitMQ Testcontainers integration test.
3. Outbox metrics and health checks.
4. Idempotent consumer foundation for future services.

## Approval Gate

Phase 8 is complete when reliable outbox publisher behavior is accepted.

Do not continue to Phase 9 until approval is received.
