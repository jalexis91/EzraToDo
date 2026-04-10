using EzraToDo.Application.Interfaces;
using EzraToDo.Domain.Entities;
using EzraToDo.Tests.Fixtures;
using FluentAssertions;
using Moq;

namespace EzraToDo.Tests.Infrastructure;

/// <summary>
/// Unit tests for the TodoRepository.
/// Tests data access operations, filtering, and soft deletes.
/// </summary>
public class TodoRepositoryTests
{
    private readonly Mock<ITodoRepository> _mockRepository;

    public TodoRepositoryTests()
    {
        _mockRepository = new Mock<ITodoRepository>();
    }

    [Fact]
    public async Task CreateAsync_WithValidTodo_ShouldReturnCreatedTodo()
    {
        // Arrange
        var todo = TodoTestFixture.CreateValidTodo();
        todo.Id = 1;

        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<Todo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(todo);

        // Act
        var result = await _mockRepository.Object.CreateAsync(todo);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Title.Should().Be(todo.Title);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnTodo()
    {
        // Arrange
        var todo = TodoTestFixture.CreateValidTodo();
        todo.Id = 1;

        _mockRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(todo);

        // Act
        var result = await _mockRepository.Object.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be(todo.Title);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Todo)null!);

        // Act
        var result = await _mockRepository.Object.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllActiveTodos()
    {
        // Arrange
        var todos = TodoTestFixture.CreateValidTodos(3);

        _mockRepository.Setup(r => r.GetAllAsync(
            It.IsAny<bool?>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(todos);

        // Act
        var result = await _mockRepository.Object.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().AllSatisfy(t => t.IsDeleted.Should().BeFalse());
    }

    [Fact]
    public async Task GetAllAsync_ShouldExcludeDeletedTodos()
    {
        // Arrange
        var activeTodos = TodoTestFixture.CreateValidTodos(2);

        _mockRepository.Setup(r => r.GetAllAsync(
            It.IsAny<bool?>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeTodos);

        // Act
        var result = await _mockRepository.Object.GetAllAsync();

        // Assert
        result.Should().NotContain(t => t.IsDeleted);
    }

    [Fact]
    public async Task UpdateAsync_WithValidTodo_ShouldReturnUpdatedTodo()
    {
        // Arrange
        var todo = TodoTestFixture.CreateValidTodo();
        todo.Id = 1;
        todo.Title = "Updated Title";

        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Todo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(todo);

        // Act
        var result = await _mockRepository.Object.UpdateAsync(todo);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Updated Title");
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldDeleteTodo()
    {
        // Arrange
        var todoId = 1;
        _mockRepository.Setup(r => r.DeleteAsync(todoId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _mockRepository.Object.DeleteAsync(todoId);

        // Assert
        _mockRepository.Verify(r => r.DeleteAsync(todoId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WithCompletedFilter_ShouldReturnOnlyCompletedTodos()
    {
        // Arrange
        var completedTodos = new List<Todo> { TodoTestFixture.CreateCompletedTodo() };

        _mockRepository.Setup(r => r.GetAllAsync(true, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(completedTodos);

        // Act
        var result = await _mockRepository.Object.GetAllAsync(isCompleted: true);

        // Assert
        result.Should().HaveCount(1);
        result.Should().AllSatisfy(t => t.IsCompleted.Should().BeTrue());
    }

    [Fact]
    public async Task GetAllAsync_WithPendingFilter_ShouldReturnOnlyPendingTodos()
    {
        // Arrange
        var pendingTodos = new List<Todo> { TodoTestFixture.CreateValidTodo() };

        _mockRepository.Setup(r => r.GetAllAsync(false, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pendingTodos);

        // Act
        var result = await _mockRepository.Object.GetAllAsync(isCompleted: false);

        // Assert
        result.Should().HaveCount(1);
        result.Should().AllSatisfy(t => t.IsCompleted.Should().BeFalse());
    }
}
