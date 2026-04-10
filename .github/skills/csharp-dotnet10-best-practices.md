# C#/.NET 10 Best Practices & Standards

This skill guide ensures EzraToDo adheres to production-ready standards for clear architecture, secure implementation, and maintainable code.

> **See Also**: [`take-home-test-objectives.md`](./take-home-test-objectives.md) for the full context on why Ezra uses the take-home as their highest-signal evaluation tool and what they're looking for in your implementation. The practices in this guide directly support meeting those objectives.

> **Note on .NET Experience**: Prior .NET experience is not required. What matters most is your engineering judgment, architecture decisions, and problem-solving approach. You're encouraged to use modern AI tools to help translate from familiar languages/frameworks into .NET.

## Architecture & Design Principles

### Clear, Straightforward Architecture
- **Layered Architecture**: Maintain clear separation between API/Controllers, Business Logic (Services), Data Access (Repositories), and Domain Models
- **Single Responsibility Principle**: Each class should have one reason to change
- **Dependency Injection**: Use constructor injection with the built-in DI container; avoid service locator pattern
- **Domain-Driven Design**: Core domain logic should be independent of frameworks and infrastructure concerns
- **Avoid Anemic Models**: Domain entities should encapsulate both data and behavior

### Production-Ready MVP Definition
A production-ready MVP must include:
- **Minimal but complete feature set**: One clear user flow fully implemented rather than many incomplete features
- **Error handling**: Comprehensive try-catch with appropriate HTTP status codes (400, 401, 403, 404, 500, etc.)
- **Input validation**: Server-side validation on all API endpoints; clear error messages for invalid requests
- **Data persistence**: Reliable database with proper schema and migrations (Entity Framework Core with migrations)
- **Security baseline**: Authentication, authorization, CORS, input sanitization, SQL injection prevention
- **Logging**: Structured logging at appropriate levels (Debug, Info, Warning, Error) for troubleshooting
- **Documentation**: README with setup instructions, API documentation (Swagger/OpenAPI), architecture overview

## Code Quality & Style

### Clean, Readable Code
- **Naming Conventions**:
  - Classes and public members: PascalCase
  - Private fields and local variables: camelCase with `_` prefix for private fields
  - Constants: PascalCase or UPPER_SNAKE_CASE
  - Async methods: Suffix with `Async`
- **Method Length**: Keep methods under 30 lines; extract complex logic into helper methods
- **Comments**: Write self-documenting code; use comments only for "why", not "what"
- **Use Modern C# 10 Features**:
  - File-scoped namespaces: `namespace MyApp.Services;`
  - Record types for immutable DTOs
  - Top-level statements for minimal console apps
  - Global using directives for frequently used namespaces
  - Required members and init-only properties
  - Null-forgiving operator judiciously: `x = y!;` (only when absolutely certain)

### Sensible Tradeoffs
- **Framework over custom code**: Use Entity Framework Core, not custom ADO.NET for data access
- **Async by default**: Use async/await for I/O operations; sync only when calling sync-only APIs
- **Configuration**: Use `appsettings.json` with environment-specific overrides; never hardcode secrets
- **Error handling**: Catch specific exceptions, not generic `Exception`; use custom exception types for domain logic
- **Performance vs. readability**: Optimize after profiling, not prematurely; readable code trumps micro-optimizations

## Testing & Validation

### Appropriate Testing Strategy
- **Unit Tests**: Test business logic in isolation; use xUnit with Moq for dependency injection
  - Arrange-Act-Assert pattern
  - Test one thing per test
  - Meaningful test names: `Should_ReturnValidationError_When_EmailIsEmpty`
  - Mock external dependencies, not business logic
- **Integration Tests**: Test API endpoints with test database; use WebApplicationFactory
  - Verify database interactions
  - Test authentication/authorization flows
  - Clean up test data after each test
- **Test Coverage**: Aim for 70%+ coverage on business logic; 50%+ overall
- **No Test Database in Production**: Use separate test configuration and database

## Security Considerations

### Authentication & Authorization
- **ASP.NET Core Identity** or **JWT**: Use built-in mechanisms, not custom implementations
  - Store passwords with bcrypt (via Identity), never in plain text
  - Use secure password requirements (min 8 chars, complexity)
  - Implement token refresh for long-lived sessions
- **CORS Policy**: Be explicit; avoid `AllowAnyOrigin` in production
  - Define allowed origins in configuration
  - Use `AllowCredentials` only when necessary
- **Authorization Attributes**: Use `[Authorize]` and `[Authorize(Policy = "...")]` on controllers/actions

### Input & Data Protection
- **Validation**: Validate all user input on the server side
  - Use Data Annotations (`[Required]`, `[StringLength]`, `[Range]`) or Fluent Validation
  - Return clear validation error messages (400 Bad Request) without exposing internal details
- **SQL Injection Prevention**: Use Entity Framework Core with parameterized queries; never concatenate SQL strings
- **Secrets Management**: Store sensitive data (API keys, connection strings) in User Secrets (dev) or Azure Key Vault (production)
  - Use `dotnet user-secrets` locally
  - Never commit secrets to version control
- **HTTPS Enforcement**: Redirect HTTP to HTTPS; set `Strict-Transport-Security` headers
- **XSS Prevention**: When returning HTML, use encoding in Razor views or return JSON for SPAs
- **CSRF Protection**: Use CSRF tokens in forms; automatic for API if using `[ValidateAntiForgeryToken]`

## Logging & Diagnostics

### Structured Logging
- **Log Framework**: Use Serilog for structured logging; configure to write to Console, File, or cloud services
- **Log Levels**:
  - **Debug**: Detailed diagnostic information; variable values, method entry/exit
  - **Information**: General flow events; successful operations, milestone checkpoints
  - **Warning**: Potential issues; deprecated API calls, recoverable errors
  - **Error**: Error events; exceptions, failed operations (user recoverable)
  - **Fatal**: Critical failures; application shutdown scenarios
- **Structured Logging Pattern**:
  ```csharp
  _logger.LogInformation("User {UserId} logged in from {IpAddress}", userId, ipAddress);
  _logger.LogError(ex, "Failed to process order {OrderId}", orderId);
  ```
- **Avoid Sensitive Data**: Don't log passwords, tokens, API keys, or PII without explicit sanitization
- **Performance**: Use lazy evaluation for expensive log operations

## API Design & Backend-Frontend Communication

### RESTful API Standards
- **Resource-Oriented URLs**:
  - `GET /api/todos` - list all todos
  - `POST /api/todos` - create a new todo
  - `GET /api/todos/{id}` - get a specific todo
  - `PUT /api/todos/{id}` - update a todo
  - `DELETE /api/todos/{id}` - delete a todo
- **HTTP Status Codes**:
  - `200 OK`: Successful GET, PUT, DELETE
  - `201 Created`: Successful POST (include Location header)
  - `204 No Content`: Successful deletion or operation with no response body
  - `400 Bad Request`: Invalid input or validation error
  - `401 Unauthorized`: Authentication required
  - `403 Forbidden`: Authenticated but not authorized
  - `404 Not Found`: Resource doesn't exist
  - `409 Conflict`: Resource state conflict (e.g., duplicate)
  - `500 Internal Server Error`: Unhandled exception
- **Error Response Format**: Use RFC 7807 Problem Details standard
  ```csharp
  // Validation error (400 Bad Request)
  {
    "type": "https://api.example.com/errors/validation-error",
    "title": "Validation Error",
    "status": 400,
    "errors": {
      "Email": ["Email is required", "Email format is invalid"]
    }
  }
  
  // Not found error (404 Not Found)
  {
    "type": "https://api.example.com/errors/not-found",
    "title": "Not Found",
    "status": 404
  }
  ```
  - Implement with custom middleware or ExceptionFilter
  - Never expose internal stack traces to clients
- **Pagination**: Include `skip` and `take` query parameters; return `X-Total-Count` header
  - Consistent ordering with optional `orderBy` and `orderDirection` parameters
- **Versioning**: Use URL versioning (`/api/v1/todos`) or header versioning; plan for backward compatibility

### Frontend-Backend Collaboration
- **Shared Models**: Keep DTOs consistent between frontend and backend; document API contracts
- **CORS Configuration**: Coordinate with frontend team on allowed origins
- **Error Messages**: Provide machine-readable error codes and human-readable messages
  - Example: `{ "code": "INVALID_EMAIL", "message": "The provided email format is invalid" }`
- **API Documentation**: Maintain Swagger/OpenAPI spec; keep it synchronized with code
  - Use `[ProducesResponseType]` attributes to document response shapes
  - Include request/response examples
- **Breaking Changes**: Deprecate old endpoints before removal; provide migration guide

## Configuration & Startup

### Program.cs Configuration Example

```csharp
var builder = WebApplicationBuilder.CreateBuilder(args);

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<TodoDbContext>(options =>
{
    options.UseSqlite(connectionString);
    if (builder.Environment.IsDevelopment())
        options.EnableSensitiveDataLogging();
});

// CORS - Be explicit about allowed origins
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins")
    .Get<string[]>() ?? new[] { "http://localhost:3000" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
    );
});

// Services
builder.Services.AddScoped<ITodoRepository, TodoRepository>();
builder.Services.AddScoped<ITodoService, TodoService>();
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware order matters!
app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigins");
app.MapControllers();

// Apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();
```

### appsettings.json Configuration

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=todos.db"
  },
  "AllowedOrigins": [
    "http://localhost:3000"
  ],
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### appsettings.Development.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Debug"
    }
  }
}
```

## Documentation & Setup

### Code Documentation
- **XML Comments**: Document public APIs
  ```csharp
  /// <summary>
  /// Retrieves a todo item by its unique identifier.
  /// </summary>
  /// <param name="id">The unique identifier of the todo.</param>
  /// <returns>The todo item if found; null otherwise.</returns>
  public Todo? GetTodoById(int id)
  ```
- **Architecture Decision Records (ADRs)**: Document significant technical decisions with context and rationale
- **README**: Include project overview, key features, technology stack, and setup instructions

### Setup Instructions
README should include:
1. **Prerequisites**: .NET 10 SDK, SQL Server (or SQLite for development), etc.
2. **Environment Setup**:
   ```bash
   git clone <repo-url>
   cd EzraToDo
   dotnet restore
   dotnet ef database update  # Apply migrations
   dotnet run
   ```
3. **Configuration**: Explain `appsettings.json`, user secrets, environment variables
4. **Running Tests**:
   ```bash
   dotnet test
   dotnet test --collect:"XPlat Code Coverage"
   ```
5. **Database Migrations**: How to create and apply migrations
   ```bash
   dotnet ef migrations add MigrationName
   dotnet ef database update
   ```
6. **API Documentation**: Link to Swagger UI (`/swagger` endpoint in development)
7. **Troubleshooting**: Common issues and solutions

## Development Workflow

### Git & Version Control
- **Meaningful Commits**: "Fix null reference in TodoService" not "fixed stuff"
- **Branch Naming**: `feature/add-todo-filtering`, `bugfix/handle-missing-todos`, `docs/api-guide`
- **Pull Request Guidelines**: Clear description, link to issues, passing tests before merge
- **Code Review**: Verify architecture, security, testing, and documentation

### Build & Deployment
- **CI/CD Pipeline**: Automate build, test, and deploy
  - Build on every push
  - Run tests on every build
  - Run code analysis (style, security)
  - Deploy to staging on PR merge
- **Versioning**: Use semantic versioning (e.g., 1.0.0-beta, 1.0.0, 1.0.1)
- **Release Notes**: Document features, fixes, and breaking changes

## Common Patterns & Anti-Patterns

### Recommended Patterns
- **Repository Pattern**: Abstract data access; use Entity Framework Core with Unit of Work if needed
- **Dependency Injection**: Constructor injection with ASP.NET Core DI container
- **Fluent Validation**: Use for complex validation logic independent of models
- **Middleware**: Custom middleware for cross-cutting concerns (logging, error handling, CORS)
- **Configuration Management**: IOptions<T> pattern for strongly-typed configuration

### Anti-Patterns to Avoid
- **Service Locator**: Don't use container directly in services; inject dependencies
- **God Services**: Don't create 1000-line service classes; split responsibilities
- **Swallowing Exceptions**: Never catch exceptions silently; log and handle appropriately
- **Synchronous Wrappers**: Avoid `.Result` or `.Wait()` on async operations; can cause deadlocks
- **Tight Coupling**: Don't depend on concrete implementations; depend on abstractions
- **Hardcoded Values**: Use configuration, constants, or enums; never magic strings/numbers

## .NET 10 Specific Features

### Modern C# 10 Features to Leverage
- **File-Scoped Namespaces**: Reduce nesting; use `namespace MyApp.Services;` at file top
- **Records**: Immutable reference types ideal for DTOs and value objects
  ```csharp
  public record CreateTodoRequest(string Title, string? Description);
  ```
- **Init-Only Properties**: Immutability at property level
  ```csharp
  public class Todo
  {
      public int Id { get; init; }
      public string Title { get; init; }
  }
  ```
- **Required Members**: Enforce property initialization
  ```csharp
  public class User
  {
      public required string Email { get; init; }
  }
  ```
- **Global Using Directives**: Centralize common namespaces in a `.GlobalUsings.cs` file
- **Implicit Using**: Enable in project file to auto-import common System namespaces

### Performance & Optimization
- **Nullable Reference Types**: Enable `<Nullable>enable</Nullable>` in project; eliminate null reference bugs
- **Async Streaming**: Use `IAsyncEnumerable<T>` for large datasets instead of returning full lists
- **Span<T> and Memory<T>**: Use for allocation-free operations on large data
- **Source Generators**: Consider for reflection-heavy scenarios (though usually not needed for typical CRUD apps)

## Review Checklist for PRs

When reviewing code, verify:
- ✓ Architecture follows layered pattern with clear concerns
- ✓ No circular dependencies
- ✓ Error handling with appropriate status codes
- ✓ Input validation on all API endpoints
- ✓ No hardcoded secrets or sensitive data
- ✓ Unit tests exist and pass; integration tests for API endpoints
- ✓ Logging at appropriate levels (no excessive Debug in production code)
- ✓ SQL queries use parameterization (Entity Framework)
- ✓ API follows REST conventions and returns correct HTTP status codes
- ✓ Documentation updated (README, XML comments, ADR if needed)
- ✓ No performance regressions (queries are efficient)
- ✓ Code is readable; methods are focused and concise
- ✓ Security: CORS, HTTPS enforced, authentication/authorization working
