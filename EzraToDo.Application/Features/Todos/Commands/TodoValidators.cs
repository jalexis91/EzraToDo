using FluentValidation;

namespace EzraToDo.Application.Features.Todos.Commands;

/// <summary>
/// Validator for CreateTodoCommand.
/// Ensures title is provided and due date is in the future.
/// </summary>
public class CreateTodoCommandValidator : AbstractValidator<CreateTodoCommand>
{
    public CreateTodoCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.UtcNow).When(x => x.DueDate.HasValue)
            .WithMessage("Due date must be in the future");
    }
}

/// <summary>
/// Validator for UpdateTodoCommand.
/// Ensures ID is valid, title is provided, and due date is in the future.
/// </summary>
public class UpdateTodoCommandValidator : AbstractValidator<UpdateTodoCommand>
{
    public UpdateTodoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.UtcNow).When(x => x.DueDate.HasValue)
            .WithMessage("Due date must be in the future");
    }
}
