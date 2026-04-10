using EzraToDo.Domain.Entities;

namespace EzraToDo.Tests.Fixtures;

/// <summary>
/// Provides reusable test data and helper methods for Todo-related tests.
/// </summary>
public class TodoTestFixture
{
    public static Todo CreateValidTodo(
        string title = "Test Todo",
        string description = "Test Description",
        DateTime? dueDate = null)
    {
        return new Todo
        {
            Title = title,
            Description = description,
            DueDate = dueDate ?? DateTime.UtcNow.AddDays(7),
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
    }

    public static List<Todo> CreateValidTodos(int count = 3)
    {
        var todos = new List<Todo>();
        for (int i = 1; i <= count; i++)
        {
            todos.Add(new Todo
            {
                Id = i,
                Title = $"Todo {i}",
                Description = $"Description {i}",
                DueDate = DateTime.UtcNow.AddDays(i),
                IsCompleted = i % 2 == 0,
                CreatedAt = DateTime.UtcNow.AddDays(-i),
                UpdatedAt = DateTime.UtcNow.AddDays(-i),
                IsDeleted = false
            });
        }
        return todos;
    }

    public static Todo CreateCompletedTodo()
    {
        var todo = CreateValidTodo();
        todo.Complete();
        return todo;
    }

    public static Todo CreateDeletedTodo()
    {
        var todo = CreateValidTodo();
        todo.Delete();
        return todo;
    }
}
