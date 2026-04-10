using Microsoft.EntityFrameworkCore;
using EzraToDo.Application.Interfaces;
using EzraToDo.Domain.Entities;
using EzraToDo.Infrastructure.Data;

namespace EzraToDo.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Todo aggregate.
/// Abstracts EF Core access patterns and encapsulates query logic.
/// Implements the Repository Pattern for clean data access.
/// </summary>
public class TodoRepository : ITodoRepository
{
    private readonly EzraTodoDbContext _context;

    public TodoRepository(EzraTodoDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves todos with optional filtering, searching, and sorting.
    /// Automatically filters out soft-deleted records.
    /// </summary>
    public async Task<IEnumerable<Todo>> GetAllAsync(
        bool? isCompleted = null,
        string? searchTerm = null,
        string? sortBy = null,
        string? sortOrder = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Todos.AsQueryable();

        // Filtering
        if (isCompleted.HasValue)
        {
            query = query.Where(t => t.IsCompleted == isCompleted.Value);
        }

        // Searching
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(t => t.Title.Contains(searchTerm) || 
                                   (t.Description != null && t.Description.Contains(searchTerm)));
        }

        // Sorting
        query = sortBy?.ToLower() switch
        {
            "title" => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(t => t.Title) : query.OrderBy(t => t.Title),
            "duedate" => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(t => t.DueDate) : query.OrderBy(t => t.DueDate),
            "iscompleted" => sortOrder?.ToLower() == "desc" ? query.OrderByDescending(t => t.IsCompleted) : query.OrderBy(t => t.IsCompleted),
            _ => sortOrder?.ToLower() == "asc" ? query.OrderBy(t => t.CreatedAt) : query.OrderByDescending(t => t.CreatedAt)
        };

        return await query.ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves a specific todo by ID if it exists and is not deleted.
    /// </summary>
    public async Task<Todo?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Todos
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Creates a new todo in the database.
    /// The database generates the ID.
    /// </summary>
    public async Task<Todo> CreateAsync(Todo todo, CancellationToken cancellationToken = default)
    {
        _context.Todos.Add(todo);
        await _context.SaveChangesAsync(cancellationToken);
        return todo;
    }

    /// <summary>
    /// Updates an existing todo in the database.
    /// The todo must already exist in the database.
    /// </summary>
    public async Task<Todo> UpdateAsync(Todo todo, CancellationToken cancellationToken = default)
    {
        _context.Todos.Update(todo);
        await _context.SaveChangesAsync(cancellationToken);
        return todo;
    }

    /// <summary>
    /// Soft deletes a todo by marking it as deleted.
    /// The record remains in the database for audit trail purposes.
    /// </summary>
    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var todo = await GetByIdAsync(id, cancellationToken);
        if (todo is null)
            return;

        todo.Delete();
        await UpdateAsync(todo, cancellationToken);
    }
}
