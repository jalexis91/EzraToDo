using Microsoft.EntityFrameworkCore;
using EzraToDo.Core.Entities;

namespace EzraToDo.Infrastructure.Data;

/// <summary>
/// Entity Framework Core DbContext for EzraToDo.
/// Handles all database operations and entity mapping.
/// </summary>
public class EzraTodoDbContext : DbContext
{
    public EzraTodoDbContext(DbContextOptions<EzraTodoDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// DbSet for Todo entities.
    /// </summary>
    public DbSet<Todo> Todos { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Todo entity
        modelBuilder.Entity<Todo>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Description)
                .HasMaxLength(2000);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Indexes for query optimization
            entity.HasIndex(e => e.IsCompleted)
                .HasDatabaseName("IX_Todo_IsCompleted");

            entity.HasIndex(e => e.IsDeleted)
                .HasDatabaseName("IX_Todo_IsDeleted");

            entity.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("IX_Todo_CreatedAt");

            entity.HasIndex(e => new { e.IsDeleted, e.IsCompleted })
                .HasDatabaseName("IX_Todo_IsDeleted_IsCompleted");

            // Global Query Filter for Soft Delete
            // Ensures deleted records are automatically excluded from all queries unless .IgnoreQueryFilters() is used.
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }
}

