# Production Readiness Guide - EzraToDo

This document outlines the steps taken to ensure EzraToDo is production-ready, the trade-offs made for the MVP, and a comprehensive roadmap for full productionization.

## ✅ Current Production Features (Implemented)
- **API Versioning (v1.0)**: URL-segment versioning (`/api/v1/todos`) for backward compatibility.
- **Centralized Validation**: `FluentValidation` with MediatR `ValidationBehavior` for consistent RFC 7807 error responses.
- **Observability**: Full OpenTelemetry (Tracing, Metrics, Logs) integrated via `EzraToDo.ServiceDefaults`.
- **Global Error Handling**: Standardized `IExceptionHandler` mapping application exceptions to Problem Details.
- **Soft Deletes**: Global EF Core query filters for `IsDeleted` records to maintain audit trails safely.
- **One-Script Local Setup**: `run-apphost.ps1` for automated dependency installation and orchestration.

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

### 1. Security & Identity
- **Full Auth Setup**: Implement **Azure AD B2C**.
- **User Management**: Add `/api/users` endpoints for registration, profile management, and RBAC (Role-Based Access Control).
- **Secure Storage**: Integrate **Azure Key Vault** for all connection strings and API keys.

### 2. Infrastructure as Code (IaC) & CI/CD
- **BICEP Setup**: Create `.bicep` templates for:
  - Azure Container Apps (API & UI)
  - Managed PostgreSQL Flexible Server
  - Azure Container Registry
  - Log Analytics Workspace & App Insights
- **GitHub Actions**: Setup workflows for:
  - `build-and-test`: Triggered on PRs.
  - `deploy-to-prod`: Triggered on merge to `main` with BICEP deployment and container push.

### 3. Monitoring & Insights
- **App Insights**: Configure deep instrumentation for dependency tracking and SQL profiling.
- **Kusto (KQL) Queries**: Pre-define dashboards for:
  - `requests | where success == false`: Identifying API failures.
  - `dependencies | where duration > 500ms`: Identifying slow DB queries.
  - `customEvents | where name == "TodoCompleted"`: Tracking user engagement.

### 4. Advanced Testing
- **Automated Integration Tests**: Implement `WebApplicationFactory` tests to verify the full API pipeline against a real (TestContainer) database.
- **E2E Testing**: Add **Playwright** or **Cypress** suites for critical UI flows (Login -> Create Todo -> Complete).

---

## 🗺️ Product Roadmap (Future Features)

### Phase 1: Efficiency & Reminders
- **Alerts & Notifications**: In-app toast notifications, Email (SendGrid), and SMS (Twilio) for overdue tasks.
- **Time Tracking**: Add `StartTime` and `EndTime` to tasks to allow for granular schedule management.

### Phase 2: Automation & Intelligence
- **Recurring Tasks**: CRON-based logic for daily/weekly/monthly task regeneration.
- **Task Templates**: "Checklists" for common flows (e.g., "Onboarding New Hire").
- **AI Tagging**: Automated categorization of tasks based on title/description.

### Phase 3: Collaboration & Workflow
- **Task Assignment**: Ability to assign tasks to other users with status tracking.
- **Sharing**: Granular permissions for sharing specific Todo lists with teams.
- **Approval Flows**: Manager "Sign-off" requirement for specific task categories.
- **Comments & Attachments**: Discussion threads and file uploads per task.
