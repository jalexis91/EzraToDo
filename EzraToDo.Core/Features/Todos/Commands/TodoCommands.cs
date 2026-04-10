using MediatR;

namespace EzraToDo.Core.Features.Todos.Commands;

/// <summary>
/// Command to create a new todo.
/// Part of the CQRS pattern - commands are write operations.
/// </summary>
public record CreateTodoCommand(
    string Title,
    string? Description,
    DateTime? DueDate
) : IRequest<CreateTodoCommandResponse>;

/// <summary>
/// Response after creating a todo.
/// </summary>
public record CreateTodoCommandResponse(
    int Id,
    string Title
);

/// <summary>
/// Command to update an existing todo.
/// </summary>
public record UpdateTodoCommand(
    int Id,
    string Title,
    string? Description,
    DateTime? DueDate
) : IRequest<UpdateTodoCommandResponse>;

/// <summary>
/// Response after updating a todo.
/// </summary>
public record UpdateTodoCommandResponse(
    int Id,
    string Title
);

/// <summary>
/// Command to complete a todo.
/// </summary>
public record CompleteTodoCommand(int Id) : IRequest;

/// <summary>
/// Command to reopen a completed todo.
/// </summary>
public record ReopenTodoCommand(int Id) : IRequest;

/// <summary>
/// Command to delete (soft delete) a todo.
/// </summary>
public record DeleteTodoCommand(int Id) : IRequest;

