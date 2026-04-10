# 🚀 Quick Start Guide - EzraToDo

## Running the Full Solution (Recommended) ⭐

To start both the API and the React UI with full observability:

```bash
# 1. Install Aspire CLI (if not already installed)
dotnet tool install -g aspire.cli

# 2. Run the solution from the project root
aspire run
```

**Services will be available at:**
- **Aspire Dashboard**: `http://localhost:18888`
- **React UI**: Port automatically assigned (check dashboard)
- **API Swagger**: `http://localhost:5039/swagger`

---

## Standalone Service Execution

### Run API Standalone
```bash
cd EzraToDo.Api
dotnet run
```
**Endpoint**: `http://localhost:5039`

### Run UI Standalone
```bash
cd ezratodo-ui
npm install
npm start
```
**Endpoint**: `http://localhost:3000` (Note: requires API running at :5039)

---

## Database (SQLite)

The solution uses a persistent file-based SQLite database (`ezratodo.db`). It is automatically created and migrated on first run.

- **Location**: `EzraToDo.Api/ezratodo.db`
- **Reset**: Delete the file and restart the API.

---

## Testing with cURL

```bash
# Get all todos
curl -X GET http://localhost:5039/api/todos

# Create a todo
curl -X POST http://localhost:5039/api/todos \
  -H "Content-Type: application/json" \
  -d '{"title":"New Task","description":"Example description"}'
```

---

## Prerequisites

- **.NET 10 SDK**
- **Node.js** (v18+)
- **Aspire CLI**

---

**Last Updated:** 2026-04-10  
**Status:** ✅ Orchestration Fixed
