# Aspire Configuration Summary - EzraToDo

## Overview
The EzraToDo project is fully configured for **.NET Aspire 13.2.2** orchestration, following production-ready best practices for a scalable and observable distributed application.

---

## ✅ Current Configuration

### 1. **AppHost Orchestration** (`EzraToDo.AppHost/Program.cs`)
**Status:** ✅ Production-Ready
- **API Service**: .NET 10 Minimal API with versioning (v1.0).
- **UI Service**: React 19 / TypeScript, integrated via `AddNpmApp`.
- **Connectivity**: Automated link between UI and versioned API (`/api/v1/todos`).
- **Observability**: Fully integrated with the Aspire Dashboard (OTLP).

### 2. **Production-Ready Features**
- **API Versioning**: URL-segment versioning (`/api/v{version}/todos`) using `Asp.Versioning.Http`.
- **Centralized Validation**: `FluentValidation` with MediatR pipeline behaviors for automatic input validation.
- **Global Error Handling**: Standardized RFC 7807 Problem Details via `IExceptionHandler`.
- **Security**: CORS hardening, HSTS, and secure headers for non-development environments.
- **Service Defaults**: Robust implementation of OpenTelemetry (Metrics, Tracing), Health Checks (Liveness/Readiness), and Resilience (Standard Resilience Handler).

### 3. **Database Architecture**
- **Local Dev**: SQLite (In-Memory with Shared Cache) for high portability and zero-setup local execution.
- **Production Roadmap**: Designed for easy migration to PostgreSQL or SQL Server.

---

## 🚀 Running the Application

### **Recommended Method (One Script)**
To automatically setup the environment, install dependencies, and run everything locally:

```powershell
# Run the orchestration script from the project root
.\run-apphost.ps1
```

**What this script does:**
1. ✅ **Setup UI**: Runs `npm install` in the frontend directory.
2. 🧹 **Cleanup**: Stops any existing EzraToDo processes to prevent port conflicts.
3. 🚀 **Orchestrate**: Starts the Aspire AppHost (API & UI).
4. 🌐 **Launch**: Automatically opens the **UI (Port 3000)**, **API Swagger**, and **Aspire Dashboard** in your default browser.

---

## 🏗️ Architecture Diagram

```
┌─────────────────────────────────────────────────┐
│      .NET Aspire Orchestrator (AppHost)         │
│                                                 │
│  ┌──────────────────┐      ┌──────────────────┐  │
│  │   ezratodo-ui    │─────▶│   EzraToDo.Api   │  │
│  │ (React Frontend) │      │  (Backend API)   │  │
│  └──────────────────┘      └──────────────────┘  │
│          ▲                          │           │
│          │                          ▼           │
│          │                 ┌──────────────────┐  │
│          └─────────────────│   SQLite (In-Mem)│  │
│                            └──────────────────┘  │
│                                                 │
│  ┌──────────────────────────────────────────┐  │
│  │     Aspire Dashboard (Port 18888)        │  │  
│  └──────────────────────────────────────────┘  │
└─────────────────────────────────────────────────┘
```

---

## 📝 Build & Versioning Status

- **API Version**: v1.0
- **Backend**: .NET 10.0
- **Frontend**: React 19.0 / TypeScript 5.0
- **Orchestration**: Aspire 13.2.2
- **Status**: ✅ **Fully Optimized & Production-Ready**

**Last Updated:** 2026-04-10
