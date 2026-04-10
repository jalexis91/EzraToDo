using MediatR;

namespace EzraToDo.Core.Features.Todos.Queries;

/// <summary>
/// Query to retrieve all todos with optional filtering, searching, and sorting.
/// Part of the CQRS pattern - queries are read operations.
/// </summary>
public record GetAllTodosQuery(
    bool? IsCompleted = null,
    string? SearchTerm = null,
    string? SortBy = null,
    string? SortOrder = null) : IRequest<GetAllTodosQueryResponse>;

/// <summary>
/// Response containing all todos.
/// </summary>
public record GetAllTodosQueryResponse(List<TodoDto> Todos);

/// <summary>
/// Query to retrieve a specific todo by ID.
/// </summary>
public record GetTodoByIdQuery(int Id) : IRequest<TodoDto?>;

/// <summary>
/// Data Transfer Object for Todo.
/// Used in API responses to decouple domain models from API contracts.
/// </summary>
public record TodoDto(
    int Id,
    string Title,
    string? Description,
    DateTime? DueDate,
    bool IsCompleted,
    DateTime? CompletedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt
)
{
    /// <summary>
    /// Maps a Domain Entity to a Data Transfer Object.
    /// Centralized to minimize duplication across CQRS handlers.
    /// </summary>
    public static TodoDto MapFromEntity(EzraToDo.Core.Entities.Todo todo) =>
        new(
            Id: todo.Id,
            Title: todo.Title,
            Description: todo.Description,
            DueDate: todo.DueDate,
            IsCompleted: todo.IsCompleted,
            CompletedAt: todo.CompletedAt,
            CreatedAt: todo.CreatedAt,
            UpdatedAt: todo.UpdatedAt
        );
}

