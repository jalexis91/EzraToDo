using EzraToDo.Application.Features.Todos.Commands;
using EzraToDo.Application.Features.Todos.Queries;
using EzraToDo.Tests.Fixtures;
using FluentAssertions;

namespace EzraToDo.Tests.Api;

/// <summary>
/// Unit tests for Todo API endpoint models and request/response handling.
/// Tests command and query contract validation and serialization.
/// </summary>
public class TodoEndpointsTests
{
    [Fact]
    public void CreateTodoCommand_WithValidData_ShouldConstruct()
    {
        // Arrange
        var title = "Test Todo";
        var description = "Test Description";
        var dueDate = DateTime.UtcNow.AddDays(7);

        // Act
        var command = new CreateTodoCommand(title, description, dueDate);

        // Assert
        command.Title.Should().Be(title);
        command.Description.Should().Be(description);
        command.DueDate.Should().Be(dueDate);
    }

    [Fact]
    public void CreateTodoCommand_WithNullDescription_ShouldConstruct()
    {
        // Arrange
        var title = "Test Todo";

        // Act
        var command = new CreateTodoCommand(title, null, DateTime.UtcNow.AddDays(7));

        // Assert
        command.Title.Should().Be(title);
        command.Description.Should().BeNull();
    }

    [Fact]
    public void CreateTodoCommand_WithNullDueDate_ShouldConstruct()
    {
        // Arrange
        var title = "Test Todo";

        // Act
        var command = new CreateTodoCommand(title, "Description", null);

        // Assert
        command.Title.Should().Be(title);
        command.DueDate.Should().BeNull();
    }

    [Fact]
    public void CreateTodoCommandResponse_ShouldConstruct()
    {
        // Arrange
        var id = 1;
        var title = "Test Todo";

        // Act
        var response = new CreateTodoCommandResponse(id, title);

        // Assert
        response.Id.Should().Be(id);
        response.Title.Should().Be(title);
    }

    [Fact]
    public void UpdateTodoCommand_WithValidData_ShouldConstruct()
    {
        // Arrange
        var id = 1;
        var title = "Updated Todo";
        var description = "Updated Description";
        var dueDate = DateTime.UtcNow.AddDays(14);

        // Act
        var command = new UpdateTodoCommand(id, title, description, dueDate);

        // Assert
        command.Id.Should().Be(id);
        command.Title.Should().Be(title);
        command.Description.Should().Be(description);
        command.DueDate.Should().Be(dueDate);
    }

    [Fact]
    public void UpdateTodoCommandResponse_ShouldConstruct()
    {
        // Arrange
        var id = 1;
        var title = "Updated Todo";

        // Act
        var response = new UpdateTodoCommandResponse(id, title);

        // Assert
        response.Id.Should().Be(id);
        response.Title.Should().Be(title);
    }

    [Fact]
    public void CompleteTodoCommand_ShouldConstruct()
    {
        // Arrange
        var id = 1;

        // Act
        var command = new CompleteTodoCommand(id);

        // Assert
        command.Id.Should().Be(id);
    }

    [Fact]
    public void ReopenTodoCommand_ShouldConstruct()
    {
        // Arrange
        var id = 1;

        // Act
        var command = new ReopenTodoCommand(id);

        // Assert
        command.Id.Should().Be(id);
    }

    [Fact]
    public void DeleteTodoCommand_ShouldConstruct()
    {
        // Arrange
        var id = 1;

        // Act
        var command = new DeleteTodoCommand(id);

        // Assert
        command.Id.Should().Be(id);
    }

    [Fact]
    public void GetAllTodosQuery_ShouldConstruct()
    {
        // Act
        var query = new GetAllTodosQuery();

        // Assert
        query.Should().NotBeNull();
    }

    [Fact]
    public void GetTodoByIdQuery_ShouldConstruct()
    {
        // Arrange
        var id = 1;

        // Act
        var query = new GetTodoByIdQuery(id);

        // Assert
        query.Id.Should().Be(id);
    }

    [Fact]
    public void TodoDto_ShouldConstruct()
    {
        // Arrange
        var id = 1;
        var title = "Test";
        var description = "Description";
        var dueDate = DateTime.UtcNow.AddDays(7);
        var isCompleted = false;
        var completedAt = (DateTime?)null;
        var createdAt = DateTime.UtcNow;
        var updatedAt = DateTime.UtcNow;

        // Act
        var dto = new TodoDto(id, title, description, dueDate, isCompleted, completedAt, createdAt, updatedAt);

        // Assert
        dto.Id.Should().Be(id);
        dto.Title.Should().Be(title);
        dto.Description.Should().Be(description);
        dto.DueDate.Should().Be(dueDate);
        dto.IsCompleted.Should().BeFalse();
        dto.CompletedAt.Should().BeNull();
    }

    [Fact]
    public void GetAllTodosQueryResponse_ShouldConstruct()
    {
        // Arrange
        var todos = TodoTestFixture.CreateValidTodos(3)
            .Select(t => new TodoDto(t.Id, t.Title, t.Description, t.DueDate, t.IsCompleted, t.CompletedAt, t.CreatedAt, t.UpdatedAt))
            .ToList();

        // Act
        var response = new GetAllTodosQueryResponse(todos);

        // Assert
        response.Todos.Should().HaveCount(3);
    }

    [Fact]
    public void GetAllTodosQueryResponse_WithEmptyTodos_ShouldConstruct()
    {
        // Arrange
        var todos = new List<TodoDto>();

        // Act
        var response = new GetAllTodosQueryResponse(todos);

        // Assert
        response.Todos.Should().BeEmpty();
    }
}
