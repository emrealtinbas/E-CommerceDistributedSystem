# Phase 7 - RabbitMQ Messaging Foundation And Outbox Preparation

## Goal

Introduce the messaging foundation required for event-driven architecture and prepare Catalog Service to publish integration events reliably through the Outbox Pattern.

This phase writes integration events to the Catalog database outbox in the same transaction as product changes. A production outbox publisher that reads these rows and publishes them to RabbitMQ will be completed in the next phase.

## What Was Added

```text
Catalog.Application
    Abstractions/Messaging
        IIntegrationEvent.cs
        IOutboxWriter.cs
    Products/IntegrationEvents
        ProductCreatedIntegrationEvent.cs
        ProductUpdatedIntegrationEvent.cs
        ProductDeactivatedIntegrationEvent.cs

Catalog.Infrastructure
    Messaging
        RabbitMqOptions.cs
    Persistence/Outbox
        OutboxMessage.cs
        OutboxWriter.cs
    Persistence/Configurations
        OutboxMessageConfiguration.cs
    Persistence/Migrations
        AddCatalogOutbox

deploy
    docker-compose.yml
```

## Messaging Boundary

Catalog now defines integration events for product changes:

```text
ProductCreatedIntegrationEvent
ProductUpdatedIntegrationEvent
ProductDeactivatedIntegrationEvent
```

Why this approach is chosen:

- Other services should not query Catalog's database.
- Product changes are business facts that can be consumed asynchronously by future services.
- Integration events are explicit contracts between services.

Problem it solves:

- Prevents hidden coupling through shared databases.
- Enables eventual consistency between services.

Alternatives:

- Other services call Catalog API synchronously when they need product data.
- Share Catalog database tables.
- Publish events directly from command handlers without an outbox.

Trade-offs:

- Events improve autonomy but introduce eventual consistency.
- Direct API calls are simpler but can create cascading failures.
- Shared databases are convenient initially but destroy microservice ownership.

Interview questions:

- What is an integration event?
- What is the difference between a domain event and an integration event?
- Why should services not read another service's database?

## Outbox Pattern Decision

Catalog command handlers now write an outbox message before `SaveChangesAsync`:

```text
Product change
    -> Add/modify Product aggregate
    -> Add OutboxMessage
    -> Commit one SQL Server transaction
    -> Invalidate cache after commit
```

Why this approach is chosen:

- SQL Server changes and RabbitMQ publishing cannot be committed atomically.
- The outbox message is stored in the same database transaction as the product change.

Problem it solves:

- Prevents the failure mode where a product is saved but the integration event is lost.
- Prevents publishing an event for a database transaction that later rolls back.

Alternatives:

- Publish directly to RabbitMQ after `SaveChangesAsync`.
- Publish before `SaveChangesAsync`.
- Use distributed transactions.
- Use event sourcing.

Trade-offs:

- Outbox adds a table, background publisher, retry logic, and cleanup requirement.
- Messages can be published more than once, so consumers must be idempotent.
- Distributed transactions are avoided because they are operationally complex and not broker-friendly in modern microservice systems.

Interview questions:

- What problem does the Outbox Pattern solve?
- Why can outbox messages be duplicated?
- Why avoid distributed transactions in microservices?

## Outbox Table

Table:

```text
OutboxMessages
```

Columns:

```text
Id
Type
Content
OccurredOnUtc
ProcessedOnUtc
Error
RetryCount
```

Why this structure is chosen:

- `Id` is the event id and supports idempotency.
- `Type` identifies the event contract.
- `Content` stores serialized event payload.
- `ProcessedOnUtc`, `Error`, and `RetryCount` support publisher lifecycle and retry behavior.

Problem it solves:

- Makes event publication observable and recoverable.

Alternatives:

- One table per event type.
- Store only payload without metadata.
- Store events in a separate event store.

Trade-offs:

- One generic outbox table is simple and flexible.
- Querying JSON content is not ideal for business reporting, but outbox is operational infrastructure, not a reporting model.

Interview questions:

- What fields should an outbox table have?
- How do you retry failed outbox messages?
- How do you clean old outbox records?

## RabbitMQ Foundation

RabbitMQ was added to Docker Compose:

```text
rabbitmq
```

Ports:

```text
5672  - AMQP
15672 - Management UI
```

Management UI:

```text
http://localhost:15672
guest / guest
```

Catalog API config now includes:

```json
"RabbitMQ": {
  "HostName": "localhost",
  "Port": 5672,
  "UserName": "guest",
  "Password": "guest",
  "ExchangeName": "ecommerce.integration"
}
```

Why this approach is chosen:

- RabbitMQ is a practical broker for command/event messaging and routing.
- Docker Compose keeps messaging infrastructure reproducible locally.

Problem it solves:

- Prepares the system for asynchronous communication without requiring manual RabbitMQ setup.

Alternatives:

- Kafka.
- Azure Service Bus or AWS SNS/SQS.
- Direct HTTP callbacks.

Trade-offs:

- RabbitMQ is excellent for routing and work queues.
- Kafka is better for high-volume event streams and replay, but heavier operationally.
- Cloud brokers reduce operations but add provider dependency.

Interview questions:

- Why choose RabbitMQ for microservice messaging?
- What is an exchange?
- What is the difference between a queue and an exchange?

## Why The Publisher Is Not Fully Implemented Yet

This phase stores outbox messages but does not yet publish them to RabbitMQ.

Why this approach is chosen:

- Reliable publishing requires retry policy, publisher confirms, idempotency, and dead-letter strategy.
- Implementing the table first makes the transactional boundary explicit before adding background processing.

Problem it solves:

- Avoids a half-reliable publisher that can lose messages or mark them processed incorrectly.

Alternatives:

- Implement full publisher immediately.
- Publish directly from command handlers.
- Use a library such as MassTransit.

Trade-offs:

- Deferring publisher means events are not yet delivered to RabbitMQ.
- The outbox foundation is now ready for a robust publisher in the next phase.

Interview questions:

- What are RabbitMQ publisher confirms?
- When should an outbox message be marked processed?
- Why do consumers still need idempotency if the producer uses outbox?

## Verification

Commands executed:

```powershell
dotnet ef migrations add AddCatalogOutbox --project "services\catalog\src\Catalog.Infrastructure\Catalog.Infrastructure.csproj" --startup-project "services\catalog\src\Catalog.Api\Catalog.Api.csproj" --output-dir "Persistence\Migrations"

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

## Recommended Phase 8

Phase 8 should implement the reliable outbox publisher:

1. Background service that reads pending outbox messages.
2. RabbitMQ exchange declaration.
3. Publisher confirms.
4. Retry and failure tracking.
5. Dead-letter queue strategy.
6. Idempotency guidance for future consumers.

## Approval Gate

Phase 7 is complete when RabbitMQ foundation and Catalog outbox preparation are accepted.

Do not continue to Phase 8 until approval is received.
