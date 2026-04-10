using EzraToDo.Core.Entities;

namespace EzraToDo.Core.Interfaces;

/// <summary>
/// Repository interface for Todo aggregate.
/// Abstracts data access logic and enables dependency injection and testing.
/// Follows the Repository Pattern for clean separation of concerns.
/// </summary>
public interface ITodoRepository
{
    /// <summary>
    /// Retrieves todos with optional filtering, searching, and sorting.
    /// </summary>
    Task<IEnumerable<Todo>> GetAllAsync(
        bool? isCompleted = null,
        string? searchTerm = null,
        string? sortBy = null,
        string? sortOrder = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a specific todo by ID if it exists and is not deleted.
    /// </summary>
    Task<Todo?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new todo in the database.
    /// </summary>
    Task<Todo> CreateAsync(Todo todo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing todo in the database.
    /// </summary>
    Task<Todo> UpdateAsync(Todo todo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes (soft delete) a todo from the database.
    /// </summary>
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}

