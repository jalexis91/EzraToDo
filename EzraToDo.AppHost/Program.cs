using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// Add the EzraToDo API service
var api = builder
    .AddProject("api", "../EzraToDo.Api/EzraToDo.Api.csproj")
    .WithHttpEndpoint(port: 5001, name: "api-https")
    .WithHttpEndpoint(port: 5000, name: "api-http")
    .WithEnvironment("DOTNET_ENVIRONMENT", "Development");

// Add the React UI project
builder.AddJavaScriptApp("ui", "../ezratodo-ui")
    .WithReference(api)
    .WithHttpEndpoint(port: 3000, env: "PORT")
    .WithExternalHttpEndpoints()
    // Point to the API endpoint
    .WithEnvironment("REACT_APP_API_URL", $"{api.GetEndpoint("api-http")}/api");

// Build and run the application
var app = builder.Build();

await app.RunAsync();
