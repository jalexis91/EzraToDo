using Microsoft.EntityFrameworkCore;
using MediatR;
using FluentValidation;
using EzraToDo.Application.Interfaces;
using EzraToDo.Application.Behaviors;
using EzraToDo.Infrastructure.Data;
using EzraToDo.Infrastructure.Repositories;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace EzraToDo.Api.Extensions;

/// <summary>
/// Extension methods for service registration.
/// Configures all dependencies for the application.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all application services including DbContext, MediatR, and repositories.
    /// Also configures Aspire service defaults like OpenTelemetry and health checks.
    /// </summary>
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var applicationAssembly = typeof(EzraToDo.Application.Features.Todos.Queries.GetAllTodosQuery).Assembly;

        // Configure Service Defaults (OpenTelemetry, Health Checks, Service Discovery)
        services.AddServiceDefaults();

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

    private static IServiceCollection AddServiceDefaults(this IServiceCollection services)
    {
        services.ConfigureOpenTelemetry();

        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        services.AddServiceDiscovery();

        services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler();
            http.AddServiceDiscovery();
        });

        return services;
    }

    private static IServiceCollection ConfigureOpenTelemetry(this IServiceCollection services)
    {
        services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();
            });

        var useOtlpExporter = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT"));
        if (useOtlpExporter)
        {
            services.AddOpenTelemetry().UseOtlpExporter();
        }

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

    /// <summary>
    /// Maps default endpoints like health checks.
    /// </summary>
    public static IEndpointRouteBuilder MapDefaultEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health");
        endpoints.MapHealthChecks("/alive", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("live")
        });

        return endpoints;
    }
}
