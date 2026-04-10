# Take-Home Test Objectives & Evaluation Criteria

This skill file outlines the objectives, requirements, and evaluation criteria for the EzraToDo take-home assessment. It ensures all deliverables meet expectations for demonstrating full-stack development competency.

## Context: Why This Take-Home Matters

Ezra uses the take-home project as a **first formal step** in their interview process—intentionally. Because resumes and short interviews don't reliably reflect technical depth, the take-home is their **highest-signal evaluation tool**. Rather than multiple early-stage interviews, Ezra prefers to evaluate real work first.

**Important**: The take-home is not used as a "volume screen." It's a serious evaluation tool that directly correlates with hiring decisions.

### What This Means for You
- If you pass the take-home, you are very likely a strong technical fit, and your odds of moving forward successfully through the rest of the interview process are strong.
- If you're excited about the role and confident in your ability to build thoughtful, production-minded systems, this is a high-leverage first step.

### What Ezra is NOT Looking For
- Flashy or over-engineered solutions
- Minimal scaffolding without real thought

### Two Common Pitfalls to Avoid
1. **Doing minimal scaffolding without real thought** - Don't just make it work; make it *thoughtful*.
2. **Over-architecting or overcomplicating** - Keep it simple and MVP-focused; avoid gold-plating.

### Key Expectation: Deeply Understand Your Code
There will be **follow-up questions** about your implementation, trade-offs, architecture, and production-readiness decisions. You should be fully comfortable **explaining and defending every part of your solution**.

This is not a test of memorized framework knowledge. **What matters most is your engineering judgment, architecture decisions, and problem-solving approach**, not prior experience in a specific language.

## Project Overview

**Goal**: Build a small to-do task management API and frontend that demonstrates:
- Backend API design and implementation
- Data structure and persistence design
- Frontend component architecture
- Frontend-backend communication
- Clean code, thoughtful architecture, and clear reasoning
- Sensible trade-offs and explicit assumptions
- Production-ready MVP features

## Evaluation Criteria

### 1. Backend API Design (.NET Core)

#### RESTful API Implementation
- **Endpoints Required**:
  - `GET /api/todos` - Retrieve all todos with optional filtering
  - `GET /api/todos/{id}` - Retrieve a specific todo
  - `POST /api/todos` - Create a new todo
  - `PUT /api/todos/{id}` - Update an existing todo
  - `DELETE /api/todos/{id}` - Delete a todo
  - `PATCH /api/todos/{id}/complete` - Toggle todo completion status (optional but recommended for MVP+)

#### HTTP Status Codes & Error Handling
- Use correct HTTP status codes:
  - `200 OK` - Successful retrieval or update
  - `201 Created` - Successful creation (include Location header)
  - `204 No Content` - Successful deletion
  - `400 Bad Request` - Validation error with clear error message
  - `404 Not Found` - Resource not found
  - `500 Internal Server Error` - Server error
- Return structured error responses using RFC 7807 Problem Details format:
  ```json
  {
    "type": "https://api.example.com/errors/validation-error",
    "title": "Validation Error",
    "status": 400,
    "errors": {
      "Title": ["Title is required", "Title must be 1-200 characters"]
    }
  }
  ```
  - For simple errors without field-level validation:
    ```json
    {
      "type": "https://api.example.com/errors/not-found",
      "title": "Not Found",
      "status": 404
    }
    ```

#### Validation & Input Handling
- Server-side validation on all inputs
- Required fields: Title (minimum 1 character, maximum 200 characters)
- Optional fields: Description (maximum 2000 characters), DueDate (future dates only)
- Clear validation error messages that guide the user

#### Testing Strategy for MVP
The MVP should include basic testing to demonstrate quality:

**Unit Tests** (Backend Services):
```csharp
[Fact]
public async Task CreateTodoAsync_WithValidTitle_ReturnsTodoWithId()
{
    // Arrange
    var service = new TodoService(mockRepository.Object);
    var request = new CreateTodoRequest { Title = "Test task" };
    
    // Act
    var result = await service.CreateTodoAsync(request);
    
    // Assert
    result.Should().NotBeNull();
    result.Id.Should().BeGreaterThan(0);
    result.Title.Should().Be("Test task");
}

[Fact]
public async Task CreateTodoAsync_WithEmptyTitle_ThrowsValidationException()
{
    // Arrange
    var service = new TodoService(mockRepository.Object);
    
    // Act & Assert
    await service.Invoking(s => s.CreateTodoAsync(new CreateTodoRequest { Title = "" }))
        .Should().ThrowAsync<ValidationException>();
}
```

**Integration Tests** (API Endpoints):
```csharp
[Fact]
public async Task CreateTodo_Endpoint_Returns201WithValidPayload()
{
    // Arrange
    var client = _factory.CreateClient();
    var payload = new { title = "Buy groceries" };
    
    // Act
    var response = await client.PostAsJsonAsync("/api/todos", payload);
    
    // Assert
    response.StatusCode.Should().Be(StatusCodes.Status201Created);
    response.Headers.Location.Should().NotBeNull();
    
    var todo = JsonSerializer.Deserialize<TodoDto>(await response.Content.ReadAsStringAsync());
    todo?.Title.Should().Be("Buy groceries");
}

[Fact]
public async Task GetTodos_Returns200WithTodoList()
{
    // Act
    var response = await _client.GetAsync("/api/todos");
    
    // Assert
    response.StatusCode.Should().Be(StatusCodes.Status200OK);
    var todos = JsonSerializer.Deserialize<List<TodoDto>>(await response.Content.ReadAsStringAsync());
    todos.Should().NotBeNull();
}
```

**Testing Targets for MVP**:
- At least 3 unit tests per service
- At least 2-3 integration tests covering CRUD operations
- Aim for 60%+ code coverage on business logic

#### Request/Response Format
- Accept and return JSON
- DTO pattern for request/response models separate from domain models
- Consistent, intuitive request/response structure
  ```json
  // Request
  {
    "title": "Complete project proposal",
    "description": "Review and finalize proposal",
    "dueDate": "2026-04-15"
  }

  // Response
  {
    "id": 1,
    "title": "Complete project proposal",
    "description": "Review and finalize proposal",
    "dueDate": "2026-04-15",
    "isCompleted": false,
    "createdAt": "2026-03-25T19:20:00Z",
    "updatedAt": "2026-03-25T19:20:00Z"
  }
  ```

#### Configuration & Startup
- Use `Program.cs` with minimal host configuration (ASP.NET Core patterns)
- Configure middleware in logical order (CORS, logging, authentication, routing)
- Use dependency injection for all services
- Provide clear API documentation (Swagger/OpenAPI)

### 2. Data Structure & Persistence Design

#### Database Choice & Migration
- **SQLite** (recommended for take-home test due to portability) or **EF Core in-memory** (fast, no setup)
- Entity Framework Core for ORM
- Include database migrations that can be applied on startup
- Schema should support:
  - Todo ID (primary key)
  - Title (required, unique is optional but not required)
  - Description (optional)
  - IsCompleted (boolean flag)
  - DueDate (optional, nullable)
  - CreatedAt (timestamp, auto-set)
  - UpdatedAt (timestamp, auto-updated)

#### Data Access Layer
- **Repository Pattern**: Implement `ITodoRepository` with methods like:
  - `GetAllAsync()` - Retrieve all todos
  - `GetByIdAsync(id)` - Retrieve a specific todo
  - `CreateAsync(todo)` - Create a new todo
  - `UpdateAsync(todo)` - Update an existing todo
  - `DeleteAsync(id)` - Delete a todo
  - `ExistsAsync(id)` - Check if todo exists
- **Unit of Work Pattern** (optional but appreciated): Coordinate multiple repository operations
- Use async/await consistently for I/O operations

#### Seeding & Demo Data
- Optional but recommended: Include seed data for demo purposes
- Useful for frontend testing without manual data creation
- Make seeding conditional (development only)

#### Query Performance
- No N+1 queries
- Use `.AsNoTracking()` for read-only queries
- Implement pagination if dealing with large datasets (optional for MVP)

### 3. Frontend Component Design (React or Vue)

#### Architecture & Components
- **Component Structure**: Logical component hierarchy with clear responsibilities
  - `TodoList` - Main container component
  - `TodoItem` - Individual todo display and interaction
  - `TodoForm` - Add/edit todo form
  - `TodoFilters` - Filter/search controls (optional)
- **State Management**: 
  - React: useState for local state, context or minimal state library for global
  - Vue: reactive() for local state, composables for logic
  - Avoid over-engineering; keep it simple for MVP
- **Code Organization**: 
  - Separate components, services, utils into different directories
  - Keep components focused and reusable

#### Features Required for MVP
- **Display todos**: List all todos with their details
- **Add todo**: Form to create new todo with title (required) and optional description, due date
- **Edit todo**: Inline editing or modal form to update existing todo
- **Delete todo**: Delete button with confirmation (recommended)
- **Complete todo**: Mark todo as complete/incomplete (toggle button or checkbox)
- **Search/Filter** (optional but recommended for MVP+):
  - Filter by completed status
  - Search by title or description
  - Sort by creation date or due date
- **Error handling**: Display validation errors and API errors clearly to user
- **Loading states**: Show loading indicators during API calls
- **Responsive design**: Mobile-friendly UI (basic Tailwind CSS or CSS modules)

#### User Experience
- **Form validation**: Client-side validation with helpful error messages
- **Feedback**: Success/error notifications for operations (create, update, delete)
- **Confirmation dialogs**: Confirm destructive operations (delete)
- **Empty state**: Clear messaging when no todos exist
- **Accessibility**: Semantic HTML, ARIA labels where needed

#### Component Code Quality
- **Props validation** (React: PropTypes or TypeScript; Vue: proper prop types)
- **Event handling**: Clear, consistent naming (onClick, onSubmit, etc.)
- **Styling**: Consistent approach (CSS modules, Tailwind, or styled-components)
- **No hardcoded values**: Use constants/config for API endpoints, timeout values, etc.

### 4. Frontend-Backend Communication

#### API Client Service
- Create a service layer that abstracts HTTP calls
  ```typescript
  // Example structure
  class TodoService {
    async getTodos(): Promise<Todo[]>
    async getTodoById(id: number): Promise<Todo>
    async createTodo(todo: CreateTodoRequest): Promise<Todo>
    async updateTodo(id: number, todo: UpdateTodoRequest): Promise<Todo>
    async deleteTodo(id: number): Promise<void>
  }
  ```
- Use fetch API or axios; consistent across the app
- Centralize API base URL configuration

#### Error Handling
- Handle network errors gracefully
- Display server error messages to user
- Log errors for debugging (console or logging service)
- Distinguish between user errors (validation) and system errors

#### Loading & Async States
- Show loading spinners during API calls
- Disable buttons/inputs during operations
- Prevent duplicate submissions (debounce/throttle if needed)
- Handle timeouts appropriately

#### CORS & Cross-Origin Setup
- Backend should have appropriate CORS headers
- Frontend should use relative URLs if hosted on same domain
- Frontend should use explicit CORS when on different domain

### 5. Clean Code & Architecture

#### Backend (.NET)
- **Layered Architecture**:
  - Controllers: API endpoints only
  - Services/Business Logic: Core todo management logic
  - Repositories: Data access
  - Models/Entities: Domain models and DTOs
- **Code Style**:
  - Meaningful variable and method names
  - No magic strings/numbers
  - Methods under 30 lines
  - DRY principle: no duplicated logic
- **Exception Handling**:
  - Catch specific exceptions, not generic Exception
  - Custom exception types for domain logic (e.g., TodoNotFoundException)
  - Translate exceptions to appropriate HTTP responses
- **Logging**:
  - Log important operations (create, update, delete)
  - Include relevant context (IDs, user actions)
  - Use appropriate log levels (Information, Warning, Error)
- **SOLID Principles**:
  - Single Responsibility: Each class has one reason to change
  - Open/Closed: Open for extension, closed for modification
  - Liskov Substitution: Implementations are interchangeable
  - Interface Segregation: Specific interfaces, not bloated ones
  - Dependency Inversion: Depend on abstractions, not concrete implementations

#### Frontend (React/Vue)
- **Component Responsibility**: Each component does one thing well
- **Naming Conventions**: 
  - Components: PascalCase
  - Variables/functions: camelCase
  - Constants: UPPER_SNAKE_CASE
- **Code Comments**: Explain "why", not "what"; keep code self-documenting
- **Avoid Common Pitfalls**:
  - Avoid deeply nested components (flatten hierarchy)
  - Avoid prop drilling (use context or state management if needed)
  - Avoid duplicate state across components
  - Avoid inline function definitions in render (performance and testing)
- **Accessibility**: 
  - Semantic HTML elements
  - ARIA labels for interactive elements
  - Keyboard navigation support
  - Color contrast compliance

### 6. Trade-offs & Assumptions Documentation

#### What to Document
Include a **ASSUMPTIONS.md** or **DESIGN_DECISIONS.md** file explaining:

**Database Choices**:
- Why SQLite vs in-memory (portability, setup complexity)
- Why Entity Framework Core (productivity, ORM benefits)
- Schema decisions (e.g., why IsCompleted is boolean vs status enum)

**Architecture Decisions**:
- Why layered architecture (separation of concerns, testability)
- Why repository pattern (data access abstraction, testability)
- Why DTOs separate from domain models (flexibility, security)

**Feature Scope**:
- What's included in MVP (e.g., no categories, no sharing, no reminders)
- What's intentionally excluded (e.g., authentication, multi-user, real-time sync)
- What could be added post-MVP

**Performance Choices**:
- Synchronous vs asynchronous operations
- In-memory state vs fetching from server
- Pagination (if implemented)
- Search implementation (client-side vs server-side filtering)

**Security Assumptions**:
- No authentication assumed for take-home (single-user app)
- No input sanitization for XSS (single user, trusted environment)
- What would change for production

**Frontend Choices**:
- Why React vs Vue
- State management approach (local state vs context vs store)
- Styling solution (Tailwind, CSS Modules, or styled-components)
- Why certain libraries were chosen (fetch vs axios, etc.)

#### Example Structure
```markdown
# Design Decisions & Assumptions

## Database
- **Choice**: SQLite with Entity Framework Core
- **Rationale**: Portability, no external dependencies, sufficient for MVP
- **Trade-off**: Not scalable for millions of todos; would use PostgreSQL in production

## Architecture
- **Pattern**: Layered (Controllers → Services → Repositories → DB)
- **Rationale**: Clear separation of concerns, testable, maintainable
- **Trade-off**: More files/structure than a small script; justified for production-ready MVP

## Frontend State
- **Approach**: React local state (useState) + context for shared state
- **Rationale**: Simple, no external dependencies, sufficient for todo app
- **Future**: Could use Redux/Zustand if state complexity grows

## Scalability & Future Work
- **Current Limitation**: Single-user, no authentication
- **Future**: Add multi-user with authentication
- **Current Limitation**: No pagination
- **Future**: Add pagination for 1000+ todos
```

### 7. README with Setup & Explanation

#### README Structure
The README.md should include:

1. **Project Title & Description**
   ```markdown
   # EzraToDo
   
   A small to-do task management application with .NET Core backend and React frontend.
   Demonstrates clean architecture, RESTful API design, and frontend-backend communication.
   ```

2. **Features**
   - List implemented features
   - Mention features included for MVP+

3. **Tech Stack**
   ```markdown
   ## Tech Stack
   
   **Backend**: .NET 10, ASP.NET Core, Entity Framework Core
   **Frontend**: React, Axios (or fetch), Tailwind CSS
   **Database**: SQLite
   **Testing**: xUnit (backend), Jest (frontend)
   ```

4. **Prerequisites**
   ```markdown
   ## Prerequisites
   
   - .NET 10 SDK
   - Node.js 18+ and npm
   - (Optional) SQL Server or SQLite browser
   ```

5. **Installation & Setup**
   ```markdown
   ## Setup Instructions
   
   ### Backend
   ```bash
   cd backend
   dotnet restore
   dotnet ef database update
   dotnet run
   ```
   API runs on http://localhost:5000
   Swagger UI: http://localhost:5000/swagger
   ```

   ### Frontend
   ```bash
   cd frontend
   npm install
   npm start
   ```
   Frontend runs on http://localhost:3000
   ```

6. **API Documentation**
   - Link to Swagger endpoint
   - or include example requests/responses

7. **Project Structure**
   ```
   EzraToDo/
   ├── backend/
   │   ├── Controllers/
   │   ├── Services/
   │   ├── Repositories/
   │   ├── Models/
   │   ├── Data/
   │   └── Program.cs
   ├── frontend/
   │   ├── src/
   │   │   ├── components/
   │   │   ├── services/
   │   │   ├── App.tsx
   │   │   └── index.tsx
   │   └── package.json
   └── README.md
   ```

8. **Usage Examples**
   - How to create, read, update, delete todos
   - Screenshots if possible

9. **Testing**
   ```markdown
   ## Running Tests
   
   ### Backend
   ```bash
   cd backend
   dotnet test
   ```

   ### Frontend
   ```bash
   cd frontend
   npm test
   ```
   ```

10. **Assumptions & Design Decisions**
    - Reference ASSUMPTIONS.md or include summary
    - Explain why certain choices were made

11. **Scalability & Future Work**
    - What would change for production
    - Potential improvements
    - Performance considerations

12. **Contributing / Notes**
    - How to extend the project
    - Key areas for improvement

### 8. Production MVP Features

Beyond the basic CRUD operations, include features that demonstrate production-ready thinking:

#### Required for Production MVP
- ✅ **Input Validation**: Server-side validation, clear error messages
- ✅ **Error Handling**: Graceful error responses with appropriate HTTP codes
- ✅ **Data Persistence**: Reliable database with migrations
- ✅ **Logging**: Track important operations
- ✅ **CORS Configuration**: Properly configured for frontend communication
- ✅ **Documentation**: Clear README and API documentation

#### Recommended for Production MVP+
- ✅ **Search/Filter**: Ability to search and filter todos
- ✅ **Sorting**: Sort by creation date, due date, or alphabetically
- ✅ **Pagination**: Handle large todo lists efficiently
- ✅ **Todo Completion**: Clear way to mark todos as complete/incomplete
- ✅ **Due Date Tracking**: Display and filter by due date
- ✅ **Deletion Confirmation**: Prevent accidental deletions
- ✅ **Loading States**: Show feedback during API calls
- ✅ **Error Messages**: Display errors to users clearly
- ✅ **Empty States**: Clear messaging when no todos exist
- ✅ **Responsive Design**: Works on mobile, tablet, desktop
- ✅ **Accessibility**: Basic accessibility features

#### Optional Enhancements (Beyond MVP)
- Category/Tags for organizing todos
- Recurring todos
- Priority levels
- Reminders/Notifications
- Dark mode
- Keyboard shortcuts
- Undo/Redo
- Bulk operations
- Export/Import todos
- Analytics/Dashboard

### 9. Scalability & Future Work

#### Current Design Limitations
Document what won't scale and why:
- Single-user (no authentication)
- In-memory or SQLite (limited to single server)
- No caching layer
- No search optimization (all todos fetched)
- No rate limiting

#### Production Roadmap
```markdown
## Scalability & Future Work

### Phase 1: Multi-User
- Add user authentication (JWT or OAuth)
- Add user isolation (users see only their todos)
- Add user management

### Phase 2: Performance
- Add Redis caching for frequently accessed todos
- Implement pagination and infinite scroll
- Add full-text search on title/description
- Implement database indexes

### Phase 3: Advanced Features
- Add categories and tags
- Add sharing/collaboration features
- Add reminders and notifications
- Add recurring todos

### Phase 4: Infrastructure
- Containerize with Docker
- Set up CI/CD pipeline
- Deploy to cloud (Azure, AWS)
- Add monitoring and alerting
- Scale to multiple servers with load balancing
```

#### Performance Considerations for Future
- Database query optimization (use EXPLAIN PLAN)
- API response caching (ETags, Cache-Control headers)
- Frontend bundle optimization (code splitting, lazy loading)
- Database connection pooling
- Asynchronous processing for heavy operations

## Submission Checklist

Before submitting the take-home test, verify:

### Code Quality
- ✓ No console.log or Debug.WriteLine left in production code
- ✓ Consistent naming conventions (PascalCase for classes, camelCase for variables)
- ✓ No hardcoded values (use configuration, constants, or enums)
- ✓ Methods are focused and under 30 lines
- ✓ DRY principle followed; no significant code duplication

### Backend
- ✓ All CRUD endpoints implemented
- ✓ Input validation on all endpoints
- ✓ Appropriate HTTP status codes
- ✓ Error responses are structured and clear
- ✓ Database migrations included
- ✓ Dependency injection configured properly
- ✓ Repository pattern or similar abstraction for data access
- ✓ Async/await used for I/O operations
- ✓ Logging in place for important operations
- ✓ Swagger/OpenAPI documentation available

### Frontend
- ✓ All CRUD operations work end-to-end
- ✓ Form validation with error messages
- ✓ Loading states during API calls
- ✓ Error handling and user-friendly error messages
- ✓ Responsive design tested on mobile
- ✓ No console errors or warnings
- ✓ Component structure is logical and maintainable
- ✓ Service layer for API communication
- ✓ No hardcoded API URLs

### Documentation
- ✓ README with clear setup instructions
- ✓ ASSUMPTIONS.md or design decisions documented
- ✓ Comments explaining non-obvious logic
- ✓ Project structure is clear and organized
- ✓ Tech stack is documented

### Testing & Verification
- ✓ Backend tests pass (if included)
- ✓ Frontend builds without errors
- ✓ API endpoints tested manually (Postman, curl, or Swagger)
- ✓ Frontend-backend communication works end-to-end
- ✓ No runtime errors in browser console
- ✓ No runtime errors in backend logs

### GitHub Repository
- ✓ Repository is public and accessible
- ✓ .gitignore prevents committing sensitive files (secrets, node_modules, bin/obj)
- ✓ Commits are meaningful (not "fix stuff" or "update")
- ✓ README is visible on repository landing page
- ✓ Repository includes setup instructions

## Evaluation Perspective

Evaluators will assess:

1. **Does it work?** End-to-end functionality without errors
2. **Is it clean?** Readable, maintainable, well-organized code
3. **Is it thoughtful?** Design decisions are sensible and documented
4. **Is it complete?** README, setup instructions, documentation
5. **Is it production-ready?** Error handling, validation, logging, MVP+ features
6. **Does it show growth?** Beyond basic requirements; extra features or polish

The goal is not perfection but demonstrating:
- Understanding of full-stack development
- Ability to make sensible trade-offs
- Clear communication through code and documentation
- Attention to user experience and code quality
- Thinking about production readiness even for a small project
