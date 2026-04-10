using EzraToDo.Application.Features.Todos.Commands;
using EzraToDo.Application.Features.Todos.Queries;
using EzraToDo.Application.Interfaces;
using EzraToDo.Domain.Entities;
using FluentAssertions;
using Moq;

namespace EzraToDo.Tests.Application;

/// <summary>
/// Unit tests for CQRS command handlers.
/// Tests validation, business logic, and repository interactions.
/// </summary>
public class TodoCommandHandlerTests
{
    private readonly Mock<ITodoRepository> _mockRepository;

    public TodoCommandHandlerTests()
    {
        _mockRepository = new Mock<ITodoRepository>();
    }

    [Fact]
    public async Task CreateTodoCommand_WithValidData_ShouldCreateAndReturnTodo()
    {
        // Arrange
        var command = new CreateTodoCommand(
            Title: "Test Todo",
            Description: "Test Description",
            DueDate: DateTime.UtcNow.AddDays(7)
        );

        var createdTodo = new Todo
        {
            Id = 1,
            Title = command.Title,
            Description = command.Description,
            DueDate = command.DueDate,
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<Todo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdTodo);

        var handler = new CreateTodoCommandHandler(_mockRepository.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Title.Should().Be(command.Title);
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<Todo>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateTodoCommand_WithEmptyTitle_ShouldThrowValidationException()
    {
        // Arrange
        var command = new CreateTodoCommand(
            Title: "",
            Description: "Test Description",
            DueDate: DateTime.UtcNow.AddDays(7)
        );

        var handler = new CreateTodoCommandHandler(_mockRepository.Object);

        // Act & Assert
        // Verify that ValidationException is thrown for empty title
        var ex = await Assert.ThrowsAsync<EzraToDo.Domain.Exceptions.ValidationException>(
            () => handler.Handle(command, CancellationToken.None));
        ex.Message.Should().Contain("Title is required");
    }

    [Fact]
    public async Task UpdateTodoCommand_WithValidData_ShouldUpdateAndReturnTodo()
    {
        // Arrange
        var todoId = 1;
        var command = new UpdateTodoCommand(
            Id: todoId,
            Title: "Updated Title",
            Description: "Updated Description",
            DueDate: DateTime.UtcNow.AddDays(14)
        );

        var existingTodo = new Todo
        {
            Id = todoId,
            Title = "Old Title",
            Description = "Old Description",
            DueDate = DateTime.UtcNow.AddDays(7),
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        var updatedTodo = new Todo
        {
            Id = todoId,
            Title = command.Title,
            Description = command.Description,
            DueDate = command.DueDate,
            IsCompleted = false,
            CreatedAt = existingTodo.CreatedAt,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        _mockRepository.Setup(r => r.GetByIdAsync(todoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTodo);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Todo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedTodo);

        var handler = new UpdateTodoCommandHandler(_mockRepository.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be(command.Title);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Todo>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTodoCommand_WithNonExistentId_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var command = new UpdateTodoCommand(
            Id: 999,
            Title: "Title",
            Description: "Description",
            DueDate: DateTime.UtcNow.AddDays(7)
        );

        _mockRepository.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Todo)null!);

        var handler = new UpdateTodoCommandHandler(_mockRepository.Object);

        // Act & Assert
        await Assert.ThrowsAsync<EzraToDo.Domain.Exceptions.EntityNotFoundException>(
            () => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task CompleteTodoCommand_WithValidTodo_ShouldMarkAsCompleted()
    {
        // Arrange
        var todoId = 1;
        var command = new CompleteTodoCommand(todoId);

        var existingTodo = new Todo
        {
            Id = todoId,
            Title = "Test",
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockRepository.Setup(r => r.GetByIdAsync(todoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTodo);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Todo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTodo);

        var handler = new CompleteTodoCommandHandler(_mockRepository.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Todo>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteTodoCommand_WithValidTodo_ShouldMarkAsDeleted()
    {
        // Arrange
        var todoId = 1;
        var command = new DeleteTodoCommand(todoId);

        var existingTodo = new Todo
        {
            Id = todoId,
            Title = "Test",
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockRepository.Setup(r => r.GetByIdAsync(todoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTodo);
        _mockRepository.Setup(r => r.DeleteAsync(todoId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new DeleteTodoCommandHandler(_mockRepository.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mockRepository.Verify(r => r.DeleteAsync(todoId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
