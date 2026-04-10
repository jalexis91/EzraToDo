# Testing Strategy & Best Practices

This skill file covers comprehensive testing strategies for the EzraToDo project, including unit tests, integration tests, and testing patterns.

> **See Also**: [`take-home-test-objectives.md`](./take-home-test-objectives.md) for the "Appropriate tests, logging, and security considerations" criterion. This guide provides concrete testing examples and patterns to achieve the recommended coverage targets.

## Testing Pyramid

```
        /\
       /  \        E2E Tests (Manual or Cypress)
      /----\       Few, slow, expensive
     /      \
    /        \
   /          \     Integration Tests (API endpoints)
  /            \    More, moderate speed
 /              \
/__________________\  Unit Tests (Services, utilities)
                    Many, fast, cheap
```

**Recommended for MVP**:
- Unit Tests: 70-80% of tests
- Integration Tests: 20-30% of tests
- E2E Tests: Manual testing (no automation needed for MVP)

## Backend Testing Strategy

### Testing Framework Setup

**Use xUnit with Moq**:
```bash
dotnet add package xunit
dotnet add package xunit.runner.visualstudio
dotnet add package Moq
dotnet add package FluentAssertions
```

### Unit Test Structure

**Pattern**: Arrange-Act-Assert (AAA)

```csharp
[Fact]
public async Task CreateTodoAsync_WithValidInput_ReturnsTodoWithId()
{
    // ARRANGE
    var mockRepository = new Mock<ITodoRepository>();
    var service = new TodoService(mockRepository.Object);
    var request = new CreateTodoRequest { Title = "Test" };

    var createdTodo = new Todo
    {
        Id = 1,
        Title = "Test",
        IsCompleted = false,
        CreatedAt = DateTime.UtcNow
    };

    mockRepository.Setup(r => r.AddAsync(It.IsAny<Todo>()))
        .ReturnsAsync(createdTodo);

    // ACT
    var result = await service.CreateTodoAsync(request);

    // ASSERT
    result.Should().NotBeNull();
    result.Id.Should().Be(1);
    result.Title.Should().Be("Test");
    mockRepository.Verify(r => r.AddAsync(It.IsAny<Todo>()), Times.Once);
}
```

### Unit Test Categories

#### 1. Happy Path Tests
Test the normal, expected behavior:

```csharp
[Fact]
public async Task GetTodoByIdAsync_WithValidId_ReturnsTodo()
{
    // Arrange
    var todoId = 1;
    var todo = new Todo { Id = todoId, Title = "Test" };
    var mockRepository = new Mock<ITodoRepository>();
    mockRepository.Setup(r => r.GetByIdAsync(todoId))
        .ReturnsAsync(todo);
    var service = new TodoService(mockRepository.Object);

    // Act
    var result = await service.GetTodoByIdAsync(todoId);

    // Assert
    result.Should().Be(todo);
}
```

#### 2. Negative Path Tests
Test error conditions:

```csharp
[Fact]
public async Task GetTodoByIdAsync_WithInvalidId_ReturnsNull()
{
    // Arrange
    var mockRepository = new Mock<ITodoRepository>();
    mockRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
        .ReturnsAsync((Todo?)null);
    var service = new TodoService(mockRepository.Object);

    // Act
    var result = await service.GetTodoByIdAsync(999);

    // Assert
    result.Should().BeNull();
}
```

#### 3. Validation Tests
Test input validation:

```csharp
[Theory]
[InlineData("")]           // Empty string
[InlineData("   ")]        // Whitespace
[InlineData(null)]         // Null
public async Task CreateTodoAsync_WithInvalidTitle_ThrowsValidationException(string? title)
{
    // Arrange
    var service = new TodoService(new Mock<ITodoRepository>().Object);

    // Act & Assert
    await service.Invoking(s => s.CreateTodoAsync(new CreateTodoRequest { Title = title! }))
        .Should().ThrowAsync<ValidationException>();
}
```

#### 4. Edge Case Tests
Test boundary conditions:

```csharp
[Fact]
public async Task CreateTodoAsync_WithMaxLengthTitle_Succeeds()
{
    // Arrange
    var maxLengthTitle = new string('a', 200);
    var service = new TodoService(new Mock<ITodoRepository>().Object);

    // Act & Assert
    await service.Invoking(s => s.CreateTodoAsync(
        new CreateTodoRequest { Title = maxLengthTitle }
    )).Should().NotThrowAsync();
}

[Fact]
public async Task CreateTodoAsync_WithTitleOverMaxLength_ThrowsValidationException()
{
    // Arrange
    var overMaxTitle = new string('a', 201);
    var service = new TodoService(new Mock<ITodoRepository>().Object);

    // Act & Assert
    await service.Invoking(s => s.CreateTodoAsync(
        new CreateTodoRequest { Title = overMaxTitle }
    )).Should().ThrowAsync<ValidationException>();
}
```

### Integration Test Structure

**Setup with WebApplicationFactory**:

```csharp
public class TodoControllerIntegrationTests : IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private HttpClient _client = null!;
    private TodoDbContext _dbContext = null!;

    public TodoControllerIntegrationTests()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove production DbContext
                    var descriptor = services
                        .SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TodoDbContext>));

                    if (descriptor != null)
                        services.Remove(descriptor);

                    // Use in-memory database for tests
                    services.AddDbContext<TodoDbContext>(options =>
                        options.UseInMemoryDatabase("TodoTestDb")
                    );
                });
            });
    }

    public async Task InitializeAsync()
    {
        _client = _factory.CreateClient();
        _dbContext = _factory.Services.GetRequiredService<TodoDbContext>();
        await _dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContext.Database.EnsureDeletedAsync();
        _factory.Dispose();
    }

    // Integration tests go here
}
```

### Integration Test Examples

```csharp
[Fact]
public async Task CreateTodo_WithValidRequest_Returns201Created()
{
    // Arrange
    var payload = new { title = "Buy groceries" };
    var jsonContent = new StringContent(
        JsonSerializer.Serialize(payload),
        Encoding.UTF8,
        "application/json"
    );

    // Act
    var response = await _client.PostAsync("/api/todos", jsonContent);

    // Assert
    response.StatusCode.Should().Be(StatusCodes.Status201Created);
    response.Headers.Location.Should().NotBeNull();

    var todo = JsonSerializer.Deserialize<TodoDto>(
        await response.Content.ReadAsStringAsync()
    );
    todo?.Title.Should().Be("Buy groceries");
}

[Fact]
public async Task CreateTodo_WithEmptyTitle_Returns400BadRequest()
{
    // Arrange
    var payload = new { title = "" };
    var jsonContent = new StringContent(
        JsonSerializer.Serialize(payload),
        Encoding.UTF8,
        "application/json"
    );

    // Act
    var response = await _client.PostAsync("/api/todos", jsonContent);

    // Assert
    response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    
    var content = await response.Content.ReadAsStringAsync();
    content.Should().Contain("validation-error");
}

[Fact]
public async Task GetTodos_WithoutFilter_Returns200WithList()
{
    // Arrange - Create test data
    var todo = new Todo { Title = "Test", IsCompleted = false };
    _dbContext.Todos.Add(todo);
    await _dbContext.SaveChangesAsync();

    // Act
    var response = await _client.GetAsync("/api/todos");

    // Assert
    response.StatusCode.Should().Be(StatusCodes.Status200OK);
    
    var todos = JsonSerializer.Deserialize<List<TodoDto>>(
        await response.Content.ReadAsStringAsync()
    );
    todos.Should().HaveCount(1);
}

[Fact]
public async Task UpdateTodo_WithValidId_Returns200()
{
    // Arrange - Create a todo first
    var createPayload = new { title = "Original" };
    var createContent = new StringContent(
        JsonSerializer.Serialize(createPayload),
        Encoding.UTF8,
        "application/json"
    );
    var createResponse = await _client.PostAsync("/api/todos", createContent);
    var createdTodo = JsonSerializer.Deserialize<TodoDto>(
        await createResponse.Content.ReadAsStringAsync()
    );

    // Act - Update it
    var updatePayload = new { title = "Updated" };
    var updateContent = new StringContent(
        JsonSerializer.Serialize(updatePayload),
        Encoding.UTF8,
        "application/json"
    );
    var updateResponse = await _client.PutAsync(
        $"/api/todos/{createdTodo?.Id}",
        updateContent
    );

    // Assert
    updateResponse.StatusCode.Should().Be(StatusCodes.Status200OK);
    
    var updated = JsonSerializer.Deserialize<TodoDto>(
        await updateResponse.Content.ReadAsStringAsync()
    );
    updated?.Title.Should().Be("Updated");
}

[Fact]
public async Task DeleteTodo_WithValidId_Returns204NoContent()
{
    // Arrange - Create a todo first
    var createPayload = new { title = "To delete" };
    var createContent = new StringContent(
        JsonSerializer.Serialize(createPayload),
        Encoding.UTF8,
        "application/json"
    );
    var createResponse = await _client.PostAsync("/api/todos", createContent);
    var createdTodo = JsonSerializer.Deserialize<TodoDto>(
        await createResponse.Content.ReadAsStringAsync()
    );

    // Act
    var deleteResponse = await _client.DeleteAsync($"/api/todos/{createdTodo?.Id}");

    // Assert
    deleteResponse.StatusCode.Should().Be(StatusCodes.Status204NoContent);

    // Verify it's actually deleted
    var getResponse = await _client.GetAsync($"/api/todos/{createdTodo?.Id}");
    getResponse.StatusCode.Should().Be(StatusCodes.Status404NotFound);
}
```

## Frontend Testing Strategy

### Testing Framework Setup (React)

```bash
npm install --save-dev @testing-library/react @testing-library/jest-dom jest
```

### Component Test Example

```typescript
// components/__tests__/TodoItem.test.tsx
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import TodoItem from '../TodoItem';
import { Todo } from '../../types/todo';

describe('TodoItem', () => {
  const mockTodo: Todo = {
    id: 1,
    title: 'Test task',
    isCompleted: false,
    createdAt: '2026-03-25T00:00:00Z',
    updatedAt: '2026-03-25T00:00:00Z',
  };

  it('renders todo title', () => {
    render(
      <TodoItem
        todo={mockTodo}
        onToggleComplete={jest.fn()}
        onDelete={jest.fn()}
      />
    );

    expect(screen.getByText('Test task')).toBeInTheDocument();
  });

  it('calls onToggleComplete when checkbox is clicked', async () => {
    const onToggleComplete = jest.fn().mockResolvedValue(undefined);

    render(
      <TodoItem
        todo={mockTodo}
        onToggleComplete={onToggleComplete}
        onDelete={jest.fn()}
      />
    );

    const checkbox = screen.getByRole('checkbox');
    fireEvent.click(checkbox);

    await waitFor(() => {
      expect(onToggleComplete).toHaveBeenCalledWith(1, false);
    });
  });

  it('shows confirmation before deleting', () => {
    const onDelete = jest.fn();
    window.confirm = jest.fn(() => true);

    render(
      <TodoItem
        todo={mockTodo}
        onToggleComplete={jest.fn()}
        onDelete={onDelete}
      />
    );

    const deleteButton = screen.getByRole('button', { name: /delete/i });
    fireEvent.click(deleteButton);

    expect(window.confirm).toHaveBeenCalled();
  });
});
```

### Hook Test Example

```typescript
// hooks/__tests__/useTodos.test.ts
import { renderHook, act, waitFor } from '@testing-library/react';
import { useTodos } from '../useTodos';
import * as todoService from '../../services/todoService';

jest.mock('../../services/todoService');

describe('useTodos', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('fetches todos on mount', async () => {
    const mockTodos = [
      { id: 1, title: 'Test', isCompleted: false, createdAt: '', updatedAt: '' },
    ];

    (todoService.todoService.getTodos as jest.Mock)
      .mockResolvedValue(mockTodos);

    const { result } = renderHook(() => useTodos());

    await waitFor(() => {
      expect(result.current.todos).toEqual(mockTodos);
    });
  });

  it('creates a new todo', async () => {
    const newTodo = { id: 1, title: 'New', isCompleted: false, createdAt: '', updatedAt: '' };

    (todoService.todoService.getTodos as jest.Mock)
      .mockResolvedValue([]);
    (todoService.todoService.createTodo as jest.Mock)
      .mockResolvedValue(newTodo);

    const { result } = renderHook(() => useTodos());

    await act(async () => {
      await result.current.createTodo({ title: 'New' });
    });

    expect(result.current.todos).toContainEqual(newTodo);
  });
});
```

## Running Tests

### Backend Tests

```bash
# Run all tests
dotnet test

# Run tests with code coverage
dotnet test /p:CollectCoverage=true

# Run specific test class
dotnet test --filter TestClass=TodoServiceTests
```

### Frontend Tests

```bash
# Run tests in watch mode
npm test

# Run tests with coverage
npm test -- --coverage

# Run tests once (CI mode)
npm test -- --watchAll=false
```

## Coverage Targets

| Category | Target | Reason |
|----------|--------|--------|
| Business Logic | 70-80% | Core functionality must be tested |
| Controllers | 50-60% | Endpoint routing, HTTP semantics |
| Utilities | 80-90% | Pure functions should be comprehensive |
| UI Components | 40-50% | Test behavior, not visual rendering |

## Test Data & Fixtures

### Backend Test Fixtures

```csharp
public class TestFixtures
{
    public static Todo CreateTodo(int id = 1, string title = "Test Todo")
    {
        return new Todo
        {
            Id = id,
            Title = title,
            Description = "Test description",
            IsCompleted = false,
            DueDate = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
    }

    public static CreateTodoRequest CreateTodoRequest(
        string title = "Test",
        string? description = null)
    {
        return new CreateTodoRequest
        {
            Title = title,
            Description = description
        };
    }
}

// Usage in tests
[Fact]
public async Task Test_Method()
{
    var todo = TestFixtures.CreateTodo(id: 5, title: "Custom Title");
    // ...
}
```

### Frontend Test Fixtures

```typescript
export const mockTodo = (overrides?: Partial<Todo>): Todo => ({
  id: 1,
  title: 'Test Todo',
  isCompleted: false,
  createdAt: '2026-03-25T00:00:00Z',
  updatedAt: '2026-03-25T00:00:00Z',
  ...overrides,
});

// Usage
const todo = mockTodo({ title: 'Custom Title' });
```

## Mocking Best Practices

### Mock APIs, Not Domain Logic

❌ **Don't mock business logic**:
```csharp
// BAD: Mocking the thing we want to test
var mockService = new Mock<ITodoService>();
mockService.Setup(s => s.IsValidTodo(todo)).Returns(true);
```

✅ **Do mock external dependencies**:
```csharp
// GOOD: Mocking the repository (external dependency)
var mockRepository = new Mock<ITodoRepository>();
mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(todo);
```

## Common Testing Mistakes

| Mistake | Problem | Solution |
|---------|---------|----------|
| Testing implementation, not behavior | Tests break when refactoring | Test public contracts, not internals |
| Not cleaning up test data | Tests fail intermittently | Use test fixtures, teardown |
| Testing too broadly | Hard to debug failures | Test one thing per test |
| Hardcoding test data | Test data gets out of sync | Use builders or factories |
| Not testing error paths | Bug coverage gaps | Include negative tests |

## Performance Testing

For MVP, manual performance testing is sufficient:

```bash
# Backend - Check response time
time curl http://localhost:5000/api/todos

# Frontend - Check bundle size
npm run build
# Check dist/ folder size
```

For future optimization, use:
- Backend: BenchmarkDotNet
- Frontend: Lighthouse CI

## Continuous Integration

### GitHub Actions Example (.github/workflows/test.yml)

```yaml
name: Tests

on: [push, pull_request]

jobs:
  backend-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '10.0'
      - run: dotnet test --verbosity normal

  frontend-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-node@v3
        with:
          node-version: '18'
      - run: npm ci
      - run: npm test -- --watchAll=false
```

## Testing Checklist

- [ ] Unit tests for all services and utils
- [ ] Integration tests for all endpoints
- [ ] Component tests for major UI components
- [ ] Error cases tested (validation, not found, server error)
- [ ] Happy path tested for each endpoint
- [ ] Test data setup/teardown working
- [ ] Coverage reports reviewed
- [ ] Tests run in CI/CD pipeline
- [ ] Test names are descriptive
- [ ] No hardcoded test data
- [ ] Async operations handled properly
- [ ] Mocks cleaned up between tests

