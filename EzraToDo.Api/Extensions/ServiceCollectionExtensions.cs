using Microsoft.EntityFrameworkCore;
using MediatR;
using FluentValidation;
using EzraToDo.Application.Interfaces;
using EzraToDo.Application.Behaviors;
using EzraToDo.Infrastructure.Data;
using EzraToDo.Infrastructure.Repositories;

namespace EzraToDo.Api.Extensions;

/// <summary>
/// Extension methods for service registration.
/// Configures all dependencies for the application.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all application services including DbContext, MediatR, and repositories.
    /// </summary>
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var applicationAssembly = typeof(EzraToDo.Application.Features.Todos.Queries.GetAllTodosQuery).Assembly;

        // Database
        services.AddDbContext<EzraTodoDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

        // MediatR for CQRS with validation behavior
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(applicationAssembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        // FluentValidation
        services.AddValidatorsFromAssembly(applicationAssembly);

        // Repositories
        services.AddScoped<ITodoRepository, TodoRepository>();

        return services;
    }

    /// <summary>
    /// Applies database migrations and initializes the database schema.
    /// </summary>
    public static IApplicationBuilder UseApplicationMigrations(
        this IApplicationBuilder app)
    {
        using (var scope = app.ApplicationServices.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<EzraTodoDbContext>();
            dbContext.Database.Migrate();
        }

        return app;
    }
}
