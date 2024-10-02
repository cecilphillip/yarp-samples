using System.Net;
using System.Net.Sockets;
using Consul;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace AspireServiceDiscovery.ItemsApi.Workers;

public class ConsulRegistrationService(
    IConsulClient consulClient,
    IConfiguration configuration,
    ILogger<ConsulRegistrationService> logger)
    : BackgroundService
{
    private AgentServiceRegistration? _serviceRegistration;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var rand = Random.Shared;
        var instanceId = rand.Next().ToString();

        try
        {
            var urlStr = configuration.GetValue<string>("ASPNETCORE_URLS")?.Split(";").First() ?? string.Empty;
            var url = new Uri(urlStr);
            
            var config = configuration.GetSection("consul:registration");
            var serviceName = config.GetValue<string>("name");
            
            // DNS
            var hName = Dns.GetHostName();
            var hAddresses = await Dns.GetHostAddressesAsync(hName, stoppingToken);
            var hAddress = hAddresses.First(ipAddr => ipAddr.AddressFamily == AddressFamily.InterNetwork);
            
            // Set config values
            _serviceRegistration = new AgentServiceRegistration
            {
                ID = $"{serviceName}-{instanceId}",
                Name = serviceName,
                // ref https://developer.hashicorp.com/consul/api-docs/agent/service#address
                Address = hAddress.ToString(),
                Port = url.Port,
                Tags = config.GetSection("tags").Get<string[]>(),
                Meta = config.GetSection("meta").GetChildren()
                    .ToDictionary(x => x.Key, x => x.Value)
            };
            
            // Health Check
            _serviceRegistration.Check = new AgentServiceCheck
            {
                CheckID = $"items-api-ttl-{instanceId}",
                Name = "ItemsAPI TTL Check",
                Notes = "60s TTL Health Check",
                Status = HealthStatus.Warning,
                TTL = TimeSpan.FromSeconds(30),
                DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(3)
            };

            var result =
                await consulClient.Agent.ServiceRegister(_serviceRegistration, stoppingToken);

            logger.LogInformation("Consul registration result => {Status}" ,result.StatusCode);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $" Unable to register service");
        }

        // TTL check report
        while (!stoppingToken.IsCancellationRequested)
        {
            await consulClient.Agent.PassTTL($"items-api-ttl-{instanceId}", "Good to go 🎉", stoppingToken);
            await Task.Delay(TimeSpan.FromSeconds(20),
                stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_serviceRegistration != null)
        {
            logger.LogInformation("Deregistering service {ServiceId}", _serviceRegistration.ID);
            await consulClient.Agent.ServiceDeregister(_serviceRegistration.ID,
                cancellationToken);
        }

        await base.StopAsync(cancellationToken);
    }
}