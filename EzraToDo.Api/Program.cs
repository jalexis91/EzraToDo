using EzraToDo.Api.Extensions;
using EzraToDo.Api.Endpoints;
using EzraToDo.ServiceDefaults;
using Asp.Versioning;
using Asp.Versioning.Conventions;

var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// ASPIRE SERVICE DEFAULTS
// ============================================================================
builder.AddServiceDefaults();

// ============================================================================
// SERVICE REGISTRATION
// ============================================================================

// Add OpenAPI/Swagger documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultPolicy", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
        else
        {
            var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [];
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
    });
});

// Add Problem Details and Global Exception Handling
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<EzraToDo.Api.Middleware.GlobalExceptionHandler>();

// Add application services (DbContext, MediatR, Repositories)
builder.Services.AddApplicationServices(builder.Configuration);

// ============================================================================
// PIPELINE CONFIGURATION
// ============================================================================

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "EzraToDo API v1");
        options.RoutePrefix = "swagger";
    });
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
    app.UseHsts();
}

app.UseCors("DefaultPolicy");

// ============================================================================
// DATABASE INITIALIZATION
// ============================================================================
app.UseApplicationMigrations();

// ============================================================================
// ENDPOINT MAPPING
// ============================================================================
app.MapDefaultEndpoints();

// Versioned API grouping
var versionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1, 0))
    .ReportApiVersions()
    .Build();

// Group for versioned endpoints
var apiV1 = app.MapGroup("/api/v{version:apiVersion}")
    .WithApiVersionSet(versionSet)
    .MapToApiVersion(1, 0);

// Map Todo endpoints to the versioned group
apiV1.MapTodoEndpoints();

// Also map to /api for UI compatibility (non-versioned fallback)
var apiBase = app.MapGroup("/api");
apiBase.MapTodoEndpoints();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
    .WithName("Health")
    .ExcludeFromDescription();

app.Run();
