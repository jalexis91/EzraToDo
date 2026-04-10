namespace EzraToDo.Core.Entities;

/// <summary>
/// Represents a to-do task in the system.
/// This is a domain entity and should encapsulate business logic related to todos.
/// </summary>
public class Todo
{
    /// <summary>
    /// Unique identifier for the todo.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Title of the todo task (required, 1-200 characters).
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Optional description of the todo task (max 2000 characters).
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Optional due date for the todo task.
    /// If set, should be a future date.
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Indicates whether the todo task has been completed.
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    /// Date and time when the todo was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the todo was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Soft delete flag. Records are marked as deleted rather than hard deleted
    /// to maintain audit trail and enable recovery.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Completed date and time (null if not yet completed).
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Business logic: Mark the todo as completed.
    /// This encapsulates the completion business rule in the domain.
    /// </summary>
    public void Complete()
    {
        if (IsCompleted)
            return;

        IsCompleted = true;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Business logic: Mark the todo as incomplete.
    /// </summary>
    public void Reopen()
    {
        if (!IsCompleted)
            return;

        IsCompleted = false;
        CompletedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Business logic: Update the todo details.
    /// </summary>
    public void Update(string title, string? description, DateTime? dueDate)
    {
        Title = title;
        Description = description;
        DueDate = dueDate;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Business logic: Soft delete the todo.
    /// </summary>
    public void Delete()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }
}

