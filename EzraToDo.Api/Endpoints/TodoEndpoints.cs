using MediatR;
using Microsoft.AspNetCore.Mvc;
using EzraToDo.Application.Features.Todos.Commands;
using EzraToDo.Application.Features.Todos.Queries;
using EzraToDo.Domain.Exceptions;

namespace EzraToDo.Api.Endpoints;

/// <summary>
/// Extension methods for mapping Todo endpoints using Minimal APIs.
/// Follows RESTful conventions and best practices for API design.
/// </summary>
public static class TodoEndpoints
{
    /// <summary>
    /// Maps all Todo-related endpoints.
    /// </summary>
    public static void MapTodoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/todos")
            .WithTags("Todos");

        group.MapGet("/", GetAllTodos)
            .Produces<GetAllTodosQueryResponse>(StatusCodes.Status200OK);

        group.MapGet("/{id:int}", GetTodoById)
            .Produces<TodoDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateTodo)
            .Produces<CreateTodoCommandResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:int}", UpdateTodo)
            .Produces<UpdateTodoCommandResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPatch("/{id:int}/complete", CompleteTodo)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPatch("/{id:int}/reopen", ReopenTodo)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:int}", DeleteTodo)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetAllTodos(
        [FromQuery] bool? isCompleted,
        [FromQuery] string? searchTerm,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortOrder,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetAllTodosQuery(isCompleted, searchTerm, sortBy, sortOrder);
        var result = await mediator.Send(query, cancellationToken);
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> GetTodoById(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetTodoByIdQuery(id), cancellationToken);
        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }

    private static async Task<IResult> CreateTodo(
        CreateTodoRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new CreateTodoCommand(request.Title, request.Description, request.DueDate);
        var result = await mediator.Send(command, cancellationToken);
        return TypedResults.Created($"/api/v1/todos/{result.Id}", result);
    }

    private static async Task<IResult> UpdateTodo(
        int id,
        UpdateTodoRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new UpdateTodoCommand(id, request.Title, request.Description, request.DueDate);
        var result = await mediator.Send(command, cancellationToken);
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> CompleteTodo(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new CompleteTodoCommand(id), cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<IResult> ReopenTodo(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new ReopenTodoCommand(id), cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<IResult> DeleteTodo(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteTodoCommand(id), cancellationToken);
        return TypedResults.NoContent();
    }
}

public record CreateTodoRequest(string Title, string? Description = null, DateTime? DueDate = null);
public record UpdateTodoRequest(string Title, string? Description = null, DateTime? DueDate = null);
