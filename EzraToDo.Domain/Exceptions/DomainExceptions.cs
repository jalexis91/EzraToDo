namespace EzraToDo.Domain.Exceptions;

/// <summary>
/// Thrown when a requested entity is not found.
/// </summary>
public class EntityNotFoundException : Exception
{
    public EntityNotFoundException(string entityName, int id)
        : base($"{entityName} with ID {id} was not found.")
    {
        EntityName = entityName;
        Id = id;
    }

    public string EntityName { get; }
    public int Id { get; }
}

/// <summary>
/// Thrown when validation of domain rules fails.
/// </summary>
public class ValidationException : Exception
{
    public ValidationException(string message) : base(message)
    {
    }

    public ValidationException(string fieldName, string message)
        : base($"Validation failed for {fieldName}: {message}")
    {
        FieldName = fieldName;
    }

    public string? FieldName { get; }
}
