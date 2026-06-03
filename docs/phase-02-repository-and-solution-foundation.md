# Phase 2 - Repository and Solution Foundation

## Goal

Create the repository foundation for a production-grade .NET 9 distributed e-commerce system without implementing business features yet.

This phase prepares the workspace so future service projects can be added consistently and incrementally.

## What Was Added

```text
E-CommerceDistributedSystem
    E-CommerceDistributedSystem.sln
    global.json
    Directory.Build.props
    Directory.Packages.props
    .editorconfig
    .gitignore
    docs
        phase-01-solution-architecture-and-service-boundaries.md
        phase-02-repository-and-solution-foundation.md
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
        rabbitmq
        sqlserver
        redis
    tests
        integration
        architecture
```

Only foundation files and placeholder `.gitkeep` files were added. No service implementation was generated in this phase.

## Architectural Decisions

### 1. Keep One Repository For The Platform

Decision:

- Use a single repository for the distributed system during this learning and build phase.

Why this approach is chosen:

- It keeps cross-service changes visible in one workspace.
- It simplifies local development with one solution and one Docker Compose environment.
- It is easier to teach architecture incrementally because all boundaries are visible together.

Problem it solves:

- Avoids early operational overhead from managing many repositories before service boundaries are proven.

Alternatives:

- One repository per microservice.
- Hybrid model with core services in separate repositories and shared infrastructure in another repository.

Trade-offs:

- A monorepo is simpler for local development but needs discipline to prevent accidental coupling.
- Polyrepo improves independent ownership but makes coordinated refactoring, local setup, and shared standards harder.

Real-world usage:

- Smaller platform teams often start with a monorepo.
- Larger organizations may move to polyrepos when team ownership and deployment pipelines mature.

Interview questions:

- Is a monorepo against microservices?
- How do you prevent tight coupling in a monorepo?
- When would you split services into separate repositories?

### 2. Use `global.json` To Pin The .NET SDK

Decision:

- Pin the SDK to `.NET 9.0.312` with `latestFeature` roll-forward.

Why this approach is chosen:

- Builds should be predictable across developer machines and CI agents.
- The project explicitly targets .NET 9.

Problem it solves:

- Prevents accidental SDK drift where different developers build the same solution using different SDK feature bands.

Alternatives:

- Do not use `global.json`.
- Pin exact SDK with no roll-forward.

Trade-offs:

- Pinning improves reproducibility but requires updates when the team upgrades SDKs.
- `latestFeature` is pragmatic because it allows compatible feature-band updates without breaking the intended major/minor SDK line.

Interview questions:

- What does `global.json` do in .NET?
- What is SDK roll-forward?
- Why can CI fail when developers do not pin SDK versions?

### 3. Use `Directory.Build.props` For Shared Build Defaults

Decision:

- Centralize common project settings:
  - `TargetFramework` = `net9.0`
  - Nullable reference types enabled
  - Implicit usings enabled
  - Latest analysis level
  - Deterministic builds enabled

Why this approach is chosen:

- Every service project should start with the same compiler and quality baseline.
- Reduces duplicated XML across dozens of projects.

Problem it solves:

- Prevents inconsistent settings between services, such as one service having nullable enabled and another not.

Alternatives:

- Repeat settings in every `.csproj`.
- Use custom SDK-style project templates.

Trade-offs:

- Central defaults are efficient but can surprise developers if they do not know props files are inherited automatically.
- Individual projects can override settings, but overrides should be rare and justified.

Interview questions:

- What is `Directory.Build.props`?
- Why enable nullable reference types in enterprise .NET?
- Should warnings be treated as errors?

### 4. Use Central Package Management

Decision:

- Add `Directory.Packages.props` and enable `ManagePackageVersionsCentrally`.

Why this approach is chosen:

- A microservice solution can contain many projects using the same packages.
- Central package versions reduce version drift and dependency conflicts.

Problem it solves:

- Avoids each service accidentally referencing different versions of EF Core, MediatR, Serilog, OpenTelemetry, or testing packages.

Alternatives:

- Put package versions in every `.csproj`.
- Use NuGet lock files only.
- Use internal company NuGet packages for all shared dependencies.

Trade-offs:

- Central package management improves consistency but means package upgrades affect multiple projects.
- Services remain independently deployable, but dependency version decisions are coordinated in this repository.

Interview questions:

- What is Central Package Management in NuGet?
- How do you avoid dependency version drift in large solutions?
- Are shared package versions a form of coupling?

### 5. Separate `building-blocks` From Services

Decision:

- Reserve `building-blocks` for carefully selected cross-cutting code:
  - `SharedKernel`
  - `Messaging`
  - `Observability`

Why this approach is chosen:

- Some concepts are intentionally shared across services, such as base domain primitives, messaging abstractions, correlation IDs, and observability setup.

Problem it solves:

- Prevents copy-pasting low-level infrastructure code into every service.

Alternatives:

- No shared code at all.
- A large shared library used by every service.
- Publish shared NuGet packages.

Trade-offs:

- Small building blocks reduce duplication.
- Large shared libraries create tight coupling and can turn microservices into a distributed monolith.
- Published NuGet packages are more realistic for enterprise environments but add package release workflow overhead.

Rule:

- Shared code must be technical or truly generic. Business rules should stay inside the owning service.

Interview questions:

- Is sharing code between microservices bad?
- What belongs in a shared kernel?
- How can shared libraries create coupling?

### 6. Use Service Folders Before Creating Projects

Decision:

- Create service folders now, but delay actual `.csproj` generation until each service phase.

Why this approach is chosen:

- It communicates the planned architecture without producing unused code.
- It keeps the implementation incremental.

Problem it solves:

- Avoids generating dozens of empty projects that add noise before architectural concepts are implemented.

Alternatives:

- Generate all service projects immediately.
- Generate only the next service folder when needed.

Trade-offs:

- Pre-created folders show intent but are placeholders.
- Generating all projects early may feel productive but usually creates maintenance overhead before value exists.

Interview questions:

- Why should architecture be implemented incrementally?
- What is the risk of generating all microservices at once?
- How do you validate service boundaries early?

### 7. Keep Deployment Assets Under `deploy`

Decision:

- Reserve `deploy` for Docker Compose and infrastructure configuration.

Why this approach is chosen:

- Local infrastructure should be versioned with the application.
- RabbitMQ, SQL Server, and Redis configuration will evolve with the system.

Problem it solves:

- Prevents hidden local setup steps and machine-specific configuration.

Alternatives:

- Keep Docker Compose at repository root only.
- Use Kubernetes manifests from the beginning.
- Use cloud-managed infrastructure only.

Trade-offs:

- Docker Compose is excellent for local development but not a production orchestrator.
- Kubernetes is closer to many enterprise production environments but too heavy for the early build phase.

Interview questions:

- Is Docker Compose production-ready?
- Why keep infrastructure configuration in source control?
- When would you introduce Kubernetes?

### 8. Create Separate Test Areas

Decision:

- Reserve `tests/integration` and `tests/architecture` for cross-service and architecture-level tests.

Why this approach is chosen:

- Unit tests should live near each service.
- Cross-cutting tests need a top-level location.

Problem it solves:

- Keeps service-owned tests separate from platform-level validation.

Alternatives:

- Put all tests in one global tests folder.
- Put every test inside each service only.

Trade-offs:

- Separate test categories improve clarity but require naming discipline.

Interview questions:

- What is an architecture test?
- What should be tested with integration tests in microservices?
- Where should contract tests live?

## Package Baseline

The central package file includes package families that match the target architecture:

| Concern | Packages |
| --- | --- |
| CQRS | MediatR |
| Validation | FluentValidation |
| Mapping | AutoMapper |
| Persistence | EF Core, SQL Server provider |
| Messaging | RabbitMQ.Client |
| Cache | StackExchange.Redis |
| Logging | Serilog |
| Tracing | OpenTelemetry |
| API Docs | Swashbuckle |
| Health Checks | ASP.NET Core health checks |
| Testing | xUnit, FluentAssertions, Testcontainers |

Important note:

- Package versions are centralized, but packages are not referenced by projects until needed.
- This avoids unused dependencies in projects while keeping a visible technology baseline.

## Current Solution State

The Visual Studio solution file exists but has no projects yet.

This is intentional for Phase 2.

Projects should be added when their phase begins. The next phase should create the first real service skeleton rather than all services at once.

## Recommended Phase 3

Phase 3 should implement the Catalog Service skeleton first.

Reason:

- Catalog has a clear domain and lower workflow complexity than ordering, payment, or inventory.
- It is a good first service to establish Clean Architecture, EF Core, CQRS, FluentValidation, AutoMapper, Swagger, Serilog, and health checks.
- Later services can reuse the same project conventions.

Proposed Phase 3 output:

```text
services/catalog/src
    Catalog.Domain
    Catalog.Application
    Catalog.Infrastructure
    Catalog.Api

services/catalog/tests
    Catalog.UnitTests
    Catalog.IntegrationTests
```

## Approval Gate

Phase 2 is complete when the repository foundation is accepted.

Do not continue to Phase 3 until approval is received.
