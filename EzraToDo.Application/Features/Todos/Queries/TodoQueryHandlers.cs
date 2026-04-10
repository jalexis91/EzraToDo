using MediatR;
using EzraToDo.Application.Interfaces;
using EzraToDo.Domain.Exceptions;

namespace EzraToDo.Application.Features.Todos.Queries;

/// <summary>
/// Handler for GetAllTodosQuery.
/// Implements the CQRS Query handler pattern.
/// Responsible for retrieving and mapping todos to DTOs.
/// </summary>
public class GetAllTodosQueryHandler : IRequestHandler<GetAllTodosQuery, GetAllTodosQueryResponse>
{
    private readonly ITodoRepository _repository;

    public GetAllTodosQueryHandler(ITodoRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetAllTodosQueryResponse> Handle(
        GetAllTodosQuery request,
        CancellationToken cancellationToken)
    {
        var todos = await _repository.GetAllAsync(
            request.IsCompleted,
            request.SearchTerm,
            request.SortBy,
            request.SortOrder,
            cancellationToken);

        var todoDtos = todos.Select(TodoDto.MapFromEntity).ToList();

        return new GetAllTodosQueryResponse(todoDtos);
    }
}

/// <summary>
/// Handler for GetTodoByIdQuery.
/// Retrieves a single todo by ID and maps it to a DTO.
/// </summary>
public class GetTodoByIdQueryHandler : IRequestHandler<GetTodoByIdQuery, TodoDto?>
{
    private readonly ITodoRepository _repository;

    public GetTodoByIdQueryHandler(ITodoRepository repository)
    {
        _repository = repository;
    }

    public async Task<TodoDto?> Handle(
        GetTodoByIdQuery request,
        CancellationToken cancellationToken)
    {
        var todo = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (todo is null)
            throw new EntityNotFoundException("Todo", request.Id);

        return TodoDto.MapFromEntity(todo);
    }
}
