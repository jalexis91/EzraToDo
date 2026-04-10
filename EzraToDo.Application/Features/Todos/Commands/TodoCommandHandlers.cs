using MediatR;
using EzraToDo.Application.Interfaces;
using EzraToDo.Domain.Entities;
using EzraToDo.Domain.Exceptions;

namespace EzraToDo.Application.Features.Todos.Commands;

/// <summary>
/// Handler for CreateTodoCommand.
/// Creates a new todo. Validation is handled by the MediatR ValidationBehavior.
/// </summary>
public class CreateTodoCommandHandler : IRequestHandler<CreateTodoCommand, CreateTodoCommandResponse>
{
    private readonly ITodoRepository _repository;

    public CreateTodoCommandHandler(ITodoRepository repository)
    {
        _repository = repository;
    }

    public async Task<CreateTodoCommandResponse> Handle(
        CreateTodoCommand request,
        CancellationToken cancellationToken)
    {
        var todo = new Todo
        {
            Title = request.Title,
            Description = request.Description,
            DueDate = request.DueDate,
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdTodo = await _repository.CreateAsync(todo, cancellationToken);

        return new CreateTodoCommandResponse(createdTodo.Id, createdTodo.Title);
    }
}

/// <summary>
/// Handler for UpdateTodoCommand.
/// Updates an existing todo. Validation is handled by the MediatR ValidationBehavior.
/// </summary>
public class UpdateTodoCommandHandler : IRequestHandler<UpdateTodoCommand, UpdateTodoCommandResponse>
{
    private readonly ITodoRepository _repository;

    public UpdateTodoCommandHandler(ITodoRepository repository)
    {
        _repository = repository;
    }

    public async Task<UpdateTodoCommandResponse> Handle(
        UpdateTodoCommand request,
        CancellationToken cancellationToken)
    {
        var todo = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (todo is null)
            throw new EntityNotFoundException("Todo", request.Id);

        todo.Update(request.Title, request.Description, request.DueDate);

        var updatedTodo = await _repository.UpdateAsync(todo, cancellationToken);

        return new UpdateTodoCommandResponse(updatedTodo.Id, updatedTodo.Title);
    }
}

/// <summary>
/// Handler for CompleteTodoCommand.
/// Marks a todo as completed.
/// </summary>
public class CompleteTodoCommandHandler : IRequestHandler<CompleteTodoCommand>
{
    private readonly ITodoRepository _repository;

    public CompleteTodoCommandHandler(ITodoRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(CompleteTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (todo is null)
            throw new EntityNotFoundException("Todo", request.Id);

        todo.Complete();

        await _repository.UpdateAsync(todo, cancellationToken);
    }
}

/// <summary>
/// Handler for ReopenTodoCommand.
/// Marks a completed todo as incomplete.
/// </summary>
public class ReopenTodoCommandHandler : IRequestHandler<ReopenTodoCommand>
{
    private readonly ITodoRepository _repository;

    public ReopenTodoCommandHandler(ITodoRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(ReopenTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (todo is null)
            throw new EntityNotFoundException("Todo", request.Id);

        todo.Reopen();

        await _repository.UpdateAsync(todo, cancellationToken);
    }
}

/// <summary>
/// Handler for DeleteTodoCommand.
/// Soft deletes a todo (marks as deleted rather than removing).
/// </summary>
public class DeleteTodoCommandHandler : IRequestHandler<DeleteTodoCommand>
{
    private readonly ITodoRepository _repository;

    public DeleteTodoCommandHandler(ITodoRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(DeleteTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (todo is null)
            throw new EntityNotFoundException("Todo", request.Id);

        await _repository.DeleteAsync(request.Id, cancellationToken);
    }
}
