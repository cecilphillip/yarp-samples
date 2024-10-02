using System.Net;
using System.Runtime.InteropServices;
using System.Text.Json;
using AspireServiceDiscovery.ServiceDefaults.ServiceEndpointProvider;
using DnsClient;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Yarp.ReverseProxy.Configuration;


var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddSingleton<IDnsQuery>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var consulAddress = config.GetValue<string>("CONSUL_HTTP_ADDR")!;
    var consulUri = new Uri(consulAddress);
    
    var endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), consulUri.Port);
    return new LookupClient(endPoint);
});

builder.Services
    .AddConsulDnsSrvServiceEndpointProvider(builder.Configuration.GetValue<string>("services:consul-server:dns-udp:0")!, options =>
    {
        options.DataCenter = builder.Configuration.GetValue<string>("CONSUL_DATACENTER")!;
    });

// Add YARP Direct Forwarding with Service Discovery support
//builder.Services.AddHttpForwarderWithServiceDiscovery();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddServiceDiscoveryDestinationResolver();

builder.Services.AddConsulClient(builder.Configuration.GetSection("consul:client"));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.MapDefaultEndpoints();
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

app.MapGet("/config/routes",  ([FromServices] IProxyConfigProvider configProvider) =>
{
    return TypedResults.Ok(configProvider.GetConfig().Routes);
});

app.MapGet("/config/clusters",  ([FromServices] IProxyConfigProvider configProvider) =>
{
    return TypedResults.Ok(configProvider.GetConfig().Clusters);
});

app.Run();
