using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace EzraToDo.ServiceDefaults;

/// <summary>
/// Extension methods for configuring Aspire service defaults.
/// Provides common observability, resilience, and health check configuration for services.
/// </summary>
public static class AspireServiceDefaults
{
    public static IServiceCollection AddServiceDefaults(this IServiceCollection services)
    {
        services.ConfigureOpenTelemetry();

        services.AddDefaultHealthChecks();

        services.AddServiceDiscovery();

        services.ConfigureHttpClientDefaults(http =>
        {
            // Turn on resilience by default
            http.AddStandardResilienceHandler();

            // Turn on service discovery by default
            http.AddServiceDiscovery();
        });

        return services;
    }

    public static WebApplicationBuilder AddServiceDefaults(this WebApplicationBuilder builder)
    {
        builder.Services.AddServiceDefaults();

        return builder;
    }

    public static IServiceCollection ConfigureOpenTelemetry(this IServiceCollection services)
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

        services.AddOpenTelemetryExporters();

        return services;
    }

    private static IServiceCollection AddOpenTelemetryExporters(this IServiceCollection services)
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT"));

        if (useOtlpExporter)
        {
            services.AddOpenTelemetry().UseOtlpExporter();
        }

        return services;
    }

    public static IServiceCollection AddDefaultHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            // Add a default liveness check to ensure app is responsive
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return services;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        // Adding health checks endpoints to applications in non-development environments has security implications.
        // See https://aka.ms/dotnet/aspire/healthchecks for details before enabling these endpoints in non-development environments.
        if (app.Environment.IsDevelopment())
        {
            // All health checks must pass for app to be considered ready to accept traffic after starting
            app.MapHealthChecks("/health");

            // Only health checks tagged with the "live" tag must pass for app to be considered alive
            app.MapHealthChecks("/alive", new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("live")
            });
        }

        return app;
    }
}
