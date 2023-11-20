using System.Runtime.InteropServices;
using System.Text.Json;
using CCProxy;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpLogging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpClientDefaults(client =>
{
    client.AddStandardResilienceHandler();
});

builder.Services.AddConsulClient(builder.Configuration.GetSection("consul:client"));

builder.Services.AddHttpLogging(logging => { logging.LoggingFields = HttpLoggingFields.All; });
builder.Services.AddHealthChecks();
builder.Services.AddReverseProxy()
    .LoadFromConsul();

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
            $"{baseUrl}/items"
        }
    };

    await ctx.Response.WriteAsJsonAsync(payload, new JsonSerializerOptions {WriteIndented = true });
});
app.MapReverseProxy();
app.Run();
