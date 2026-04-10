# EzraToDo - Take-Home Test Project

A **full-stack task management application** built for **.NET 10**, **React 19**, and **.NET Aspire**.

---

## 🚀 Quick Start (Local Setup)

### Build & Run with One Script (Recommended)

To automatically setup the environment, install dependencies, and run everything locally:

```powershell
# Run the orchestration script (PowerShell required)
.\run-apphost.ps1
```

### Build & Run with Aspire (Alternative) ⭐

To automatically setup the environment, install dependencies, and run everything locally:

```powershell
# From the project root
aspire run
```

This will:
1. ✅ **Setup UI**: Run `npm install` in the `ezratodo-ui` directory.
2. 🚀 **Orchestrate**: Start the Aspire AppHost (API & UI).
3. 🌐 **Launch**: Dashboard at `http://localhost:18888`.

---

## ⚖️ Architectural Trade-offs (MVP vs. Production)

| Feature | MVP Implementation | Production Requirement | Rationale for Trade-off |
| :--- | :--- | :--- | :--- |
| **Database** | SQLite (In-Memory/Shared) | PostgreSQL / Azure SQL | Prioritized "zero-setup" local execution for evaluation. |
| **Auth** | None (Anonymous) | OpenID Connect / OAuth2 | Simplified focus on core business logic and CQRS patterns. |
| **Hosting** | Local Aspire Host | Azure Container Apps / K8s | Reduced infrastructure complexity for the initial prototype. |
| **UI State** | Local React State | Redux / React Query | Local state is sufficient for the current limited entity count. |
| **Caching** | None | Distributed Redis | Unnecessary for single-user scale; adds infrastructure overhead. |

---

## 🚀 The Path to Production (Action Plan)

For detailed productionization steps including **BICEP templates**, **CI/CD pipelines**, and **Kusto (KQL)** queries for App Insights, please see:

📖 **[PRODUCTION-READINESS.md](./PRODUCTION-READINESS.md)**

### Key Requirements for Full Production Readiness:
1. **Security & Identity**: Implement Duende IdentityServer or Azure AD B2C with full user management and RBAC.
2. **IaC & CI/CD**: Setup BICEP infrastructure and GitHub Actions for automated, multi-stage deployments.
3. **Monitoring**: Deep App Insights instrumentation with custom dashboards for failure and engagement tracking.
4. **Automated Testing**: Implement automated integration tests using `WebApplicationFactory` and E2E testing with Playwright.

---

## 🗺️ Product Roadmap (Future Features)

- ✅ **Phase 1: Efficiency & Reminders**: In-app/Email/SMS alerts for overdue tasks and granular time tracking.
- ✅ **Phase 2: Automation & Intelligence**: Recurring tasks, "Checklist" templates, and automated AI categorization.
- ✅ **Phase 3: Collaboration & Workflow**: Task assignment, granular sharing permissions, manager approval flows, and task comments/attachments.

---

## 🏗️ Architecture

### Layered Architecture (4 Layers)

```
┌─────────────────────────────────────────┐
│    API Layer (Presentation)             │ ← REST endpoints, middleware
│    EzraToDo.Api                         │
├─────────────────────────────────────────┤
│    Application Layer (Business Logic)   │ ← CQRS commands/queries
│    EzraToDo.Application                 │
├─────────────────────────────────────────┤
│    Infrastructure Layer (Data Access)   │ ← EF Core, repositories
│    EzraToDo.Infrastructure              │
├─────────────────────────────────────────┤
│    Domain Layer (Core Business)         │ ← Entities, business rules
│    EzraToDo.Domain                      │
└─────────────────────────────────────────┘
```

### Design Patterns

- **CQRS (Command Query Responsibility Segregation)**
  - Separates write operations (Commands) from read operations (Queries)
  - Each has dedicated handlers
  - Orchestrated via MediatR

- **Repository Pattern**
  - Abstracts data access logic
  - Enables testing with mock repositories
  - Decouples domain from infrastructure

- **Dependency Injection**
  - Built-in .NET DI container
  - Loose coupling between layers
  - Easy to test and extend

---

## 📡 API Endpoints (v1.0)

| Method | Endpoint | Purpose | Status Code |
|--------|----------|---------|------------|
| **GET** | `/api/v1/todos` | Get all active todos | 200 |
| **GET** | `/api/v1/todos/{id}` | Get a specific todo | 200, 404 |
| **POST** | `/api/v1/todos` | Create a new todo | 201, 400 |
| **PUT** | `/api/v1/todos/{id}` | Update a todo | 200, 400, 404 |
| **PATCH** | `/api/v1/todos/{id}/complete` | Mark as completed | 204, 404 |
| **PATCH** | `/api/v1/todos/{id}/reopen` | Reopen a completed todo | 204, 404 |
| **DELETE** | `/api/v1/todos/{id}` | Delete (soft delete) a todo | 204, 404 |

---

## 💾 Technology Stack

| Component | Technology | Version | Purpose |
|-----------|-----------|---------|---------|
| **Runtime** | .NET | 10.0 | Latest framework |
| **Web Framework** | ASP.NET Core | 10.0 | Web API host |
| **Frontend** | React | 19.0 | UI Layer |
| **Orchestration**| .NET Aspire | 13.2.2 | Service management |
| **ORM** | Entity Framework Core | 10.0.0 | Database access |
| **Database** | SQLite | In-Memory | Shared cache in-memory DB |

---

## 📚 Documentation

- **[PRODUCTION-READINESS.md](./PRODUCTION-READINESS.md)** - Production roadmap & trade-offs
- **[ASPIRE-SETUP.md](./ASPIRE-SETUP.md)** - Service orchestration details
- **[PROJECT-INDEX.md](./PROJECT-INDEX.md)** - Complete project overview
- **[CQRS-API-IMPLEMENTATION.md](./CQRS-API-IMPLEMENTATION.md)** - Detailed architecture guide

---

**Last Updated:** 2026-04-10  
**Status:** ✅ Production-Ready Optimized
