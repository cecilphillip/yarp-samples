using System.Runtime.InteropServices;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpLogging;
using Yarp.ReverseProxy.Model;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpLogging(logging => { logging.LoggingFields = HttpLoggingFields.All; });
builder.Services.AddHealthChecks();
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();
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

app.MapGet("/", async ctx =>
{
    var baseUrl = $"{ctx.Request.Scheme}://{ctx.Request.Host}";
    var payload = new
    {
        AvailableRoutes = new[]
        {
            $"{baseUrl}/items", $"{baseUrl}/addresses"
        }
    };

    await ctx.Response.WriteAsJsonAsync(payload, new JsonSerializerOptions {WriteIndented = true });
});
app.MapReverseProxy();
app.Run();
