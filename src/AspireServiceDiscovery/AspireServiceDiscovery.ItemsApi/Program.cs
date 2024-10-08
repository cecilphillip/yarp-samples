using System.Runtime.InteropServices;
using System.Text.Json;
using AspireServiceDiscovery.ItemsApi;
using AspireServiceDiscovery.ItemsApi.Workers;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddConsulClient(builder.Configuration.GetSection("consul:client"));
builder.Services.AddHostedService<ConsulRegistrationService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opts =>
{
    opts.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Items API",
    });
});

builder.Services.AddHttpLogging(logging => { logging.LoggingFields = HttpLoggingFields.All; });

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.DisplayOperationId();
});

app.UseHttpLogging();
app.UseHealthChecks("/status", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        var json = JsonSerializer.Serialize(new
        {
            Status = report.Status.ToString(),
            Environment = builder.Environment.EnvironmentName,
            Application = builder.Environment.ApplicationName,
            Platform = RuntimeInformation.FrameworkDescription
        });

        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(json);
    }
});

app.MapGroup("/api/items")
    .MapItemApis()
    .AddEndpointFilter<LbInfoFilter>()
    .WithName("Items API")
    .WithOpenApi();

app.Run();