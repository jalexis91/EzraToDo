# 🚀 Aspire Setup & Orchestration

This guide explains how to manage and run the EzraToDo application using .NET Aspire 13.2.2.

## Overview

**EzraToDo** uses **.NET Aspire** to orchestrate:
- **api** - RESTful Backend (.NET 10)
- **ui** - Frontend (React/TypeScript)
- **Observability** - Built-in Dashboard for logs, metrics, and traces.

---

## Prerequisites

- **.NET 10 SDK**
- **Node.js** (v18+)
- **Aspire CLI**: `dotnet tool install -g aspire.cli`

---

## Running the Solution

The recommended way to run the project is using the Aspire CLI:

```bash
# Run from the repository root
aspire run
```

### What happens behind the scenes?
1. **Dependency Check**: Aspire ensures Node.js and .NET are available.
2. **UI Installer**: Runs `npm install` automatically if `node_modules` is missing.
3. **API Build**: Builds the .NET projects.
4. **Dashboard**: Starts the developer dashboard at `http://localhost:18888`.
5. **Service Launch**: Starts the API and UI with automatic port discovery.

---

## AppHost Configuration

The orchestration logic is defined in `EzraToDo.AppHost/Program.cs`:

```csharp
using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// API Service
var api = builder
    .AddProject("api", "../EzraToDo.Api/EzraToDo.Api.csproj")
    .WithHttpEndpoint(port: 5001, name: "api-https")
    .WithHttpEndpoint(port: 5000, name: "api-http");

// UI Service (React)
builder.AddJavaScriptApp("ui", "../ezratodo-ui")
    .WithReference(api)
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints()
    .WithEnvironment("REACT_APP_API_URL", $"{api.GetEndpoint("api-http")}/api");

builder.Build().Run();
```

---

## Troubleshooting

### 1. UI Fails to Start ("node not found")
Ensure `node` is in your system `PATH`. If using a version manager (like nvm), ensure the current shell has access to the node executable.

### 2. UI Dependency Conflicts
If `npm install` fails during orchestration, navigate to `ezratodo-ui` and run:
```bash
npm install --legacy-peer-deps
```
This project uses an `.npmrc` to handle these automatically, but manual intervention might be needed on some systems.

### 3. "dev" script missing
The UI project requires a `"dev": "react-scripts start"` entry in `package.json` for Aspire's `AddJavaScriptApp` to work correctly.

### 4. API Returns 500 Errors
- Ensure `ezratodo.db` exists or the API has write permissions to the directory.
- Check the **Console Logs** in the Aspire Dashboard for detailed stack traces.

---

## Monitoring

Once running, access the dashboard at `http://localhost:18888` to:
- **Resources**: Monitor CPU/RAM usage of each service.
- **Console**: View real-time logs from both the API and UI.
- **Structured Logs**: Search through structured OpenTelemetry logs.
- **Traces**: Inspect the full lifecycle of requests from UI -> API.

---

**Last Updated:** 2026-04-10  
**Aspire Version:** 13.2.2  
**Status:** ✅ Fully Configured
