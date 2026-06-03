# Phase 1 - Solution Architecture and Service Boundaries

## Goal

Build a production-grade distributed e-commerce platform with pragmatic microservices, Clean Architecture, DDD, CQRS, and event-driven workflows.

This phase does not generate service projects or implementation code. The purpose is to define clear service boundaries, ownership, communication rules, data boundaries, and the first set of architectural decisions before writing code.

## Target System

The platform supports the following business flow:

1. Customer places an order.
2. Inventory reserves stock.
3. Payment charges the customer.
4. Order is completed.
5. Notification sends confirmation email.

Failure flow:

1. Any step fails.
2. Saga compensation starts.
3. Reserved stock is released.
4. Payment is refunded if already charged.
5. Order is cancelled.

## High-Level Architecture

The system will be composed of independently deployable services. Each service owns its own data model and database. Services communicate through HTTP for synchronous queries/commands where necessary and RabbitMQ events for asynchronous business workflows.

```text
Client / API Consumer
        |
        v
API Gateway or direct service routing in early phases
        |
        +--> Identity Service
        +--> Catalog Service
        +--> Basket Service
        +--> Order Service
        +--> Payment Service
        +--> Inventory Service
        +--> Notification Service

RabbitMQ
        |
        +--> Integration Events
        +--> Saga Messages
        +--> Dead Letter Queues

Redis
        |
        +--> Distributed Cache
        +--> Basket storage candidate
        +--> Idempotency key storage candidate

SQL Server
        |
        +--> Separate database per service
```

## Service Boundaries

### 1. Identity Service

Responsibilities:

- Own customer, user, role, and authentication data.
- Issue and validate JWT access tokens.
- Manage user registration and login.
- Provide identity claims to other services through tokens, not direct database access.

Owns:

- Users
- Roles
- Credentials
- Refresh tokens if implemented

Does not own:

- Orders
- Payments
- Basket contents
- Product catalog

Database:

- `IdentityDb`

Key architectural decision:

- Identity is isolated because authentication data has different security, auditing, and compliance requirements from commerce data.

Alternatives:

- Use external identity provider such as Keycloak, Auth0, Azure AD B2C.
- Put identity inside a monolith or shared user database.

Trade-offs:

- Custom identity gives learning value and control, but increases security responsibility.
- External identity reduces security risk but hides implementation details and adds vendor/runtime dependency.

Interview questions:

- Why should authentication be isolated from business services?
- What should be stored in JWT claims?
- Why should services not query the identity database directly?

### 2. Catalog Service

Responsibilities:

- Own product information shown to customers.
- Manage product name, description, category, price, images, and active status.
- Provide product read APIs for browsing and search.

Owns:

- Product
- Category
- Product price as display/sales price

Does not own:

- Physical stock quantity
- Customer basket
- Order lifecycle

Database:

- `CatalogDb`

Key architectural decision:

- Catalog and Inventory are separated because product information and stock movement change for different reasons.

Alternatives:

- Merge catalog and inventory into one product service.
- Use search engine as source of truth.

Trade-offs:

- Separation improves ownership and scaling but requires eventual consistency between product and stock availability.
- Merging is simpler but creates a large service with mixed business responsibilities.

Interview questions:

- Why is stock not part of the catalog aggregate?
- How do you handle product price changes after an order is placed?
- What data should be duplicated into an order line?

### 3. Basket Service

Responsibilities:

- Own temporary customer shopping basket state.
- Add, update, and remove basket items.
- Prepare basket snapshot for checkout.

Owns:

- Basket
- Basket items

Does not own:

- Product master data
- Final order records
- Payment state

Database:

- `BasketDb` if persistence is SQL-based.
- Redis may be used as the primary basket store if requirements accept volatile/cache-like storage with persistence configuration.

Key architectural decision:

- Basket is separated because it has different lifecycle, load profile, and consistency requirements from orders.

Alternatives:

- Store basket in Order Service as draft orders.
- Store basket only client-side.
- Store basket only in Redis.

Trade-offs:

- Separate Basket Service scales well for high read/write traffic but adds another service boundary.
- Draft orders simplify checkout but pollute order domain with temporary state.
- Client-side basket is simple but unsafe for price and product validation.

Interview questions:

- Why should checkout revalidate basket prices and product availability?
- Is Redis enough for basket persistence?
- What happens if a product is removed while it is in a basket?

### 4. Order Service

Responsibilities:

- Own order lifecycle.
- Create orders from basket checkout.
- Track order status transitions.
- Coordinate the order saga.
- Store immutable order line snapshots.

Owns:

- Order
- Order line
- Order status
- Saga state for order processing

Does not own:

- Product master data
- Stock quantity
- Payment provider transaction execution
- Email delivery

Database:

- `OrderDb`

Key architectural decision:

- Order Service is the saga orchestrator because order completion is the central business process and order status must reflect the full workflow.

Alternatives:

- Choreography-based saga where services react to events without a central orchestrator.
- Put saga orchestration in a dedicated workflow service.

Trade-offs:

- Orchestration is easier to understand, monitor, and debug.
- Choreography reduces central coupling but can become difficult to trace in complex flows.
- Dedicated workflow service is powerful but adds infrastructure and conceptual complexity.

Interview questions:

- Why does an order line store product name and price snapshots?
- What is the difference between order state and payment state?
- When would you choose orchestration over choreography?

### 5. Payment Service

Responsibilities:

- Own payment transaction state.
- Charge customer payment method.
- Refund successful charges during compensation.
- Enforce idempotency for charge and refund operations.

Owns:

- Payment
- Payment transaction
- Refund transaction
- External payment provider reference

Does not own:

- Order status as source of truth
- Customer identity credentials
- Inventory stock

Database:

- `PaymentDb`

Key architectural decision:

- Payment is isolated because it integrates with external providers and needs strict idempotency, auditing, and failure handling.

Alternatives:

- Payment logic inside Order Service.
- Direct payment provider calls from frontend.

Trade-offs:

- Separate Payment Service improves isolation and auditability but introduces asynchronous consistency with orders.
- Putting payment inside Order Service is simpler but couples order workflow to provider-specific concerns.

Interview questions:

- Why must payment operations be idempotent?
- How do you handle provider timeout after a successful charge?
- What is the difference between authorization, capture, charge, and refund?

### 6. Inventory Service

Responsibilities:

- Own stock quantity and reservations.
- Reserve stock for an order.
- Release reservations during compensation.
- Confirm stock deduction after successful payment if required by domain rules.

Owns:

- Inventory item
- Stock reservation
- Stock movement

Does not own:

- Product description
- Product images
- Order lifecycle

Database:

- `InventoryDb`

Key architectural decision:

- Inventory is isolated because stock consistency is a critical business invariant and should not be modified by other services.

Alternatives:

- Keep stock quantity in Catalog Service.
- Use event-sourced inventory from the beginning.

Trade-offs:

- Separate Inventory Service protects stock invariants but requires careful reservation and compensation logic.
- Event sourcing is excellent for stock movement history but increases implementation complexity.

Interview questions:

- Why reserve stock instead of directly decrementing it?
- How does optimistic concurrency prevent overselling?
- What happens when two customers buy the last item at the same time?

### 7. Notification Service

Responsibilities:

- Own notification sending process.
- Send email after order completion or cancellation.
- Track notification attempts and failures.

Owns:

- Notification message
- Delivery status
- Retry attempts

Does not own:

- Order business rules
- Customer credentials
- Payment state

Database:

- `NotificationDb`

Key architectural decision:

- Notifications are asynchronous because email delivery should not block checkout completion.

Alternatives:

- Send emails directly from Order Service.
- Use third-party event automation only.

Trade-offs:

- Separate service improves resilience and retry handling but adds eventual consistency.
- Direct sending is simpler but can make order processing slow or fragile.

Interview questions:

- Why should email sending be asynchronous?
- How do you avoid sending duplicate emails?
- What is the role of retry and dead letter queues in notifications?

## Data Ownership Rules

Rules:

- Each microservice owns exactly one database.
- No service may read or write another service's database.
- Cross-service data access happens through APIs or integration events.
- Data duplication is allowed when it creates local autonomy and performance.
- Duplicated data must be treated as eventually consistent unless explicitly synchronized.

Why this approach is chosen:

- Database-per-service enforces service autonomy and prevents hidden coupling.
- Teams can evolve schemas independently.
- Services can scale, deploy, and recover independently.

Problem solved:

- Avoids distributed monoliths where services exist but remain coupled through shared tables.

Alternatives:

- Shared database with schema-per-service.
- Single database for all services.
- Database views or cross-database queries.

Trade-offs:

- Strong local ownership improves autonomy but makes reporting and cross-service queries harder.
- Eventual consistency must be accepted and designed explicitly.

Interview questions:

- Why is a shared database dangerous in microservices?
- How do services query data owned by another service?
- How do you handle reporting across multiple service databases?

## Communication Model

### Synchronous Communication

Used for:

- Authentication token validation through standard JWT validation.
- Simple read operations when immediate data is required.
- Administrative operations in controlled scenarios.

Avoid using synchronous calls for:

- Long-running checkout workflow.
- Payment completion chains.
- Inventory reservation plus payment plus notification in one request.

### Asynchronous Communication

Used for:

- Order lifecycle events.
- Inventory reservation events.
- Payment events.
- Notification events.
- Saga commands and replies.

RabbitMQ will be used as the message broker.

Why this approach is chosen:

- The checkout process crosses multiple service boundaries and cannot be made reliable with a single database transaction.
- Async messaging improves resilience when downstream services are temporarily unavailable.

Problem solved:

- Prevents cascading failures and request timeouts during multi-step business workflows.

Alternatives:

- Direct HTTP calls between all services.
- gRPC for service-to-service communication.
- Kafka instead of RabbitMQ.

Trade-offs:

- RabbitMQ is practical for command/event messaging and routing but requires careful retry, dead letter, and idempotency handling.
- HTTP is easier to debug but less resilient for long-running workflows.
- Kafka is strong for event streams but heavier for command-style workflows.

Interview questions:

- When should microservices communicate synchronously vs asynchronously?
- What is eventual consistency?
- Why does messaging require idempotent consumers?

## Checkout Saga Boundary

The Order Service will orchestrate the checkout saga.

Initial saga steps:

```text
Order Service
    -> Create Pending Order
    -> Publish ReserveStock command

Inventory Service
    -> Reserve stock
    -> Publish StockReserved or StockReservationFailed

Order Service
    -> If stock reserved, publish ChargePayment command
    -> If stock failed, cancel order

Payment Service
    -> Charge payment
    -> Publish PaymentCharged or PaymentFailed

Order Service
    -> If payment charged, mark order Completed and publish OrderCompleted
    -> If payment failed, publish ReleaseStock and mark order Cancelled

Notification Service
    -> Send order confirmation or cancellation email
```

Why orchestration is chosen:

- The order process has a clear owner: Order Service.
- It is easier to answer where the workflow state lives.
- Compensation is explicit and traceable.

Problem solved:

- Avoids scattered business process logic across services.

Alternatives:

- Choreographed saga through pure events.
- External workflow engine such as Temporal, MassTransit state machine, Dapr Workflow, or Azure Durable Functions.

Trade-offs:

- Orchestration introduces dependency from Order Service to workflow message contracts.
- Choreography has less central control but can become hard to understand.
- Workflow engines are robust but add operational complexity.

Interview questions:

- What is a saga?
- Why can we not use a distributed transaction here?
- What is compensation in a saga?

## Clean Architecture Per Service

Each service will follow this internal structure:

```text
ServiceName
    src
        ServiceName.Domain
        ServiceName.Application
        ServiceName.Infrastructure
        ServiceName.Api
    tests
        ServiceName.UnitTests
        ServiceName.IntegrationTests
```

Layer responsibilities:

| Layer | Responsibility | Depends On |
| --- | --- | --- |
| Domain | Entities, value objects, aggregates, domain events, business rules | Nothing |
| Application | Use cases, CQRS handlers, validators, DTOs, interfaces | Domain |
| Infrastructure | EF Core, repositories, RabbitMQ, Redis, external providers, outbox | Application, Domain |
| API | Controllers/endpoints, authentication, middleware, Swagger, health checks | Application, Infrastructure |

Why this approach is chosen:

- Business rules stay independent from frameworks.
- Infrastructure can be replaced without rewriting use cases.
- Tests can target domain and application behavior without running the full system.

Problem solved:

- Prevents controllers and EF Core models from becoming the center of the application.

Alternatives:

- Vertical Slice Architecture.
- Traditional layered architecture.
- Minimal API with feature folders only.

Trade-offs:

- Clean Architecture adds project and dependency structure overhead.
- For small services, too many projects can feel heavy.
- For enterprise systems, explicit boundaries improve maintainability.

Interview questions:

- Why should Domain not depend on Infrastructure?
- Where should validation live?
- What is the difference between Domain and Application layers?

## CQRS Boundary

CQRS will be applied pragmatically inside services.

Rules:

- Commands change state.
- Queries read state.
- Commands and queries use separate request/handler models with MediatR.
- We will not start with separate read and write databases unless a service has a real need.

Why this approach is chosen:

- It separates intent and improves use case clarity.
- It avoids overengineering with separate physical models too early.

Problem solved:

- Prevents large service classes with mixed read/write responsibilities.

Alternatives:

- CRUD services only.
- Full CQRS with separate read database.
- Event sourcing.

Trade-offs:

- Handler-per-use-case creates more files.
- Full CQRS improves read scaling but adds synchronization complexity.

Interview questions:

- Does CQRS always require separate databases?
- What is the difference between command validation and domain validation?
- Why use MediatR instead of calling services directly?

## Outbox Pattern Boundary

Services that publish integration events after database changes will use the Outbox Pattern.

Initial candidates:

- Order Service
- Inventory Service
- Payment Service
- Notification Service if it emits delivery events

Why this approach is chosen:

- Database changes and message publishing cannot be committed atomically without distributed transactions.
- Outbox stores messages in the same database transaction as business state.

Problem solved:

- Prevents the classic bug where an order is saved but the event is not published, or an event is published but the database transaction rolls back.

Alternatives:

- Publish directly after `SaveChanges`.
- Distributed transactions with MSDTC.
- Event sourcing.

Trade-offs:

- Outbox requires a background publisher and cleanup strategy.
- Messages may be published more than once, so consumers must be idempotent.

Interview questions:

- What problem does the Outbox Pattern solve?
- Why can outbox messages be duplicated?
- How do you design idempotent consumers?

## Idempotency Boundary

Idempotency is required for:

- Payment charge requests.
- Payment refunds.
- Inventory reservations.
- Message consumers.
- Checkout submission if client retries.

Why this approach is chosen:

- Distributed systems retry operations. Retried operations must not create duplicate side effects.

Problem solved:

- Avoids duplicate payments, duplicate stock reservations, and duplicate notifications.

Alternatives:

- Rely on frontend disabling buttons.
- Rely on message broker exactly-once delivery.
- Use only database unique constraints without idempotency records.

Trade-offs:

- Idempotency requires key design, storage, expiration, and response replay strategy.
- It adds persistence overhead but is mandatory for payment-grade reliability.

Interview questions:

- What is idempotency?
- Why is exactly-once delivery usually unrealistic?
- Where should idempotency keys come from?

## Distributed Cache Boundary

Redis will be used for selected scenarios, not as a replacement for all databases.

Initial candidates:

- Product read cache in Catalog Service.
- Basket storage or basket cache.
- Idempotency key cache where durable persistence is not required.
- Token/session metadata if needed.

Why this approach is chosen:

- Redis reduces database load and improves latency for hot reads or temporary state.

Problem solved:

- Avoids repeatedly querying SQL Server for high-traffic read data.

Alternatives:

- In-memory cache per service instance.
- SQL Server only.
- CDN/search index for catalog reads.

Trade-offs:

- Redis introduces cache invalidation and availability considerations.
- In-memory cache is simpler but does not work consistently across multiple instances.

Interview questions:

- What is the difference between distributed cache and in-memory cache?
- How do you invalidate product cache after price change?
- Should Redis be the source of truth?

## Observability Boundary

Every service will include:

- Serilog structured logging.
- OpenTelemetry tracing and metrics.
- Health checks.
- Correlation IDs across HTTP and messaging.

Why this approach is chosen:

- Distributed systems are hard to debug without logs, traces, metrics, and health signals.

Problem solved:

- Enables root cause analysis across service boundaries.

Alternatives:

- Plain text logs only.
- Logging only at API gateway.
- Manual correlation through request IDs only.

Trade-offs:

- Observability has runtime and storage cost.
- Too much logging can expose sensitive data or make noise.

Interview questions:

- What is distributed tracing?
- Why are correlation IDs important?
- What is the difference between liveness and readiness checks?

## Initial Repository Structure

The repository will evolve toward this structure:

```text
E-CommerceDistributedSystem
    E-CommerceDistributedSystem.sln
    docs
        phase-01-solution-architecture-and-service-boundaries.md
    building-blocks
        SharedKernel
        Messaging
        Observability
    services
        identity
        catalog
        basket
        ordering
        payment
        inventory
        notification
    deploy
        docker-compose.yml
        rabbitmq
        sqlserver
        redis
    tests
        integration
        architecture
```

Important note:

- This is the target structure, not yet generated in this phase.
- In Phase 2 we should create the solution folders and the first service skeleton incrementally.

## Why Not Build Everything At Once

Building all services immediately would create a large amount of code before validating the architecture. Enterprise systems are built incrementally because each boundary and pattern should be proven with one or two services first.

Recommended incremental path:

1. Phase 2: Repository and solution foundation.
2. Phase 3: Catalog Service with Clean Architecture, EF Core, CQRS, validation, Swagger, health checks.
3. Phase 4: Basket Service with Redis and checkout preparation.
4. Phase 5: Order Service with order aggregate and outbox.
5. Phase 6: RabbitMQ messaging foundation.
6. Phase 7: Inventory reservation with optimistic concurrency.
7. Phase 8: Payment with idempotency and retry policies.
8. Phase 9: Saga orchestration and compensation.
9. Phase 10: Notification with retries and DLQ.
10. Phase 11: Docker Compose and full local environment.
11. Phase 12: Observability hardening with OpenTelemetry and Serilog.

## Phase 1 Decisions Summary

| Decision | Choice |
| --- | --- |
| Architecture style | Microservices with Clean Architecture per service |
| Domain approach | Pragmatic DDD |
| Data ownership | Database per service |
| Workflow style | Orchestrated saga owned by Order Service |
| Messaging | RabbitMQ for async commands/events |
| Cache | Redis for distributed cache and selected temporary state |
| Persistence | SQL Server with EF Core |
| API style | ASP.NET Core Web API |
| CQRS | MediatR-based command/query separation inside services |
| Reliability | Outbox, idempotency, retry, DLQ, health checks |
| Observability | Serilog, OpenTelemetry, correlation IDs |

## Approval Gate

Phase 1 is complete when the service boundaries and architectural direction are accepted.

Do not continue to Phase 2 until approval is received.

Recommended next phase after approval:

Phase 2 - Repository and Solution Foundation.
