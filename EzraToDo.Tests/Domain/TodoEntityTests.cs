using EzraToDo.Domain.Entities;
using FluentAssertions;

namespace EzraToDo.Tests.Domain;

/// <summary>
/// Unit tests for the Todo domain entity.
/// Tests business logic and validation in the domain model.
/// </summary>
public class TodoEntityTests
{
    [Fact]
    public void CreateTodo_WithValidData_ShouldSucceed()
    {
        // Arrange
        var title = "Complete project proposal";
        var description = "Finish the Q1 project proposal";
        var dueDate = DateTime.UtcNow.AddDays(7);

        // Act
        var todo = new Todo
        {
            Title = title,
            Description = description,
            DueDate = dueDate,
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        // Assert
        todo.Title.Should().Be(title);
        todo.Description.Should().Be(description);
        todo.DueDate.Should().Be(dueDate);
        todo.IsCompleted.Should().BeFalse();
        todo.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Complete_WithValidTodo_ShouldMarkAsCompleted()
    {
        // Arrange
        var todo = new Todo
        {
            Title = "Test",
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        todo.Complete();

        // Assert
        todo.IsCompleted.Should().BeTrue();
        todo.CompletedAt.Should().HaveValue();
        todo.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Complete_WhenAlreadyCompleted_ShouldStayCompleted()
    {
        // Arrange
        var todo = new Todo
        {
            Title = "Test",
            IsCompleted = true,
            CompletedAt = DateTime.UtcNow.AddHours(-1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var previousCompletedAt = todo.CompletedAt;

        // Act
        todo.Complete();

        // Assert
        todo.IsCompleted.Should().BeTrue();
        todo.CompletedAt.Should().Be(previousCompletedAt);
    }

    [Fact]
    public void Reopen_WithCompletedTodo_ShouldMarkAsIncomplete()
    {
        // Arrange
        var todo = new Todo
        {
            Title = "Test",
            IsCompleted = true,
            CompletedAt = DateTime.UtcNow.AddHours(-1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        todo.Reopen();

        // Assert
        todo.IsCompleted.Should().BeFalse();
        todo.CompletedAt.Should().BeNull();
    }

    [Fact]
    public void Delete_WithValidTodo_ShouldMarkAsDeleted()
    {
        // Arrange
        var todo = new Todo
        {
            Title = "Test",
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        todo.Delete();

        // Assert
        todo.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void UpdateDueDate_WithValidDate_ShouldUpdate()
    {
        // Arrange
        var todo = new Todo
        {
            Title = "Test",
            DueDate = DateTime.UtcNow.AddDays(1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var newDueDate = DateTime.UtcNow.AddDays(14);

        // Act
        todo.DueDate = newDueDate;

        // Assert
        todo.DueDate.Should().Be(newDueDate);
    }
}
