using EzraToDo.Application.Features.Todos.Queries;
using EzraToDo.Application.Interfaces;
using EzraToDo.Domain.Entities;
using EzraToDo.Tests.Fixtures;
using FluentAssertions;
using Moq;

namespace EzraToDo.Tests.Application;

/// <summary>
/// Unit tests for CQRS query handlers.
/// Tests data retrieval and query logic.
/// </summary>
public class TodoQueryHandlerTests
{
    private readonly Mock<ITodoRepository> _mockRepository;

    public TodoQueryHandlerTests()
    {
        _mockRepository = new Mock<ITodoRepository>();
    }

    [Fact]
    public async Task GetAllTodosQuery_ShouldReturnAllActiveTodos()
    {
        // Arrange
        var todos = TodoTestFixture.CreateValidTodos(3);
        var query = new GetAllTodosQuery();

        _mockRepository.Setup(r => r.GetAllAsync(
            It.IsAny<bool?>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(todos);

        var handler = new GetAllTodosQueryHandler(_mockRepository.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Todos.Should().HaveCount(3);
        _mockRepository.Verify(r => r.GetAllAsync(
            null, null, null, null,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllTodosQuery_WithFiltering_ShouldPassParametersToRepository()
    {
        // Arrange
        var query = new GetAllTodosQuery(IsCompleted: true, SearchTerm: "test", SortBy: "title", SortOrder: "desc");
        var todos = TodoTestFixture.CreateValidTodos(1);

        _mockRepository.Setup(r => r.GetAllAsync(
            true, "test", "title", "desc",
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(todos);

        var handler = new GetAllTodosQueryHandler(_mockRepository.Object);

        // Act
        await handler.Handle(query, CancellationToken.None);

        // Assert
        _mockRepository.Verify(r => r.GetAllAsync(
            true, "test", "title", "desc",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllTodosQuery_WithEmptyRepository_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetAllTodosQuery();

        _mockRepository.Setup(r => r.GetAllAsync(
            It.IsAny<bool?>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Todo>());

        var handler = new GetAllTodosQueryHandler(_mockRepository.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Todos.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTodoByIdQuery_WithValidId_ShouldReturnTodo()
    {
        // Arrange
        var todo = TodoTestFixture.CreateValidTodo();
        todo.Id = 1;
        var query = new GetTodoByIdQuery(1);

        _mockRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(todo);

        var handler = new GetTodoByIdQueryHandler(_mockRepository.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Title.Should().Be(todo.Title);
        _mockRepository.Verify(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetTodoByIdQuery_WithInvalidId_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var query = new GetTodoByIdQuery(999);

        _mockRepository.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Todo)null!);

        var handler = new GetTodoByIdQueryHandler(_mockRepository.Object);

        // Act & Assert
        await Assert.ThrowsAsync<EzraToDo.Domain.Exceptions.EntityNotFoundException>(
            () => handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task GetAllTodosQuery_ShouldExcludeDeletedTodos()
    {
        // Arrange
        var activeTodos = TodoTestFixture.CreateValidTodos(2);
        var query = new GetAllTodosQuery();

        _mockRepository.Setup(r => r.GetAllAsync(
            It.IsAny<bool?>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeTodos.Where(t => !t.IsDeleted).ToList());

        var handler = new GetAllTodosQueryHandler(_mockRepository.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Todos.Should().HaveCount(2);
    }
}
