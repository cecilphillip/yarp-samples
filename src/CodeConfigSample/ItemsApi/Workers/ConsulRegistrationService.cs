using System.Net;
using Consul;

namespace ItemsApi.Workers
{
    public class ConsulRegistrationService(
        IConsulClient consulClient,
        IConfiguration configuration,
        ILogger<ConsulRegistrationService> logger)
        : BackgroundService
    {
        private readonly IConfigurationSection _config = configuration.GetSection("consul:registration");
        private AgentServiceRegistration? _serviceRegistration;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                // Set config values
                _serviceRegistration = new AgentServiceRegistration
                {
                    Name = _config.GetValue<string>("name"),
                    Port = _config.GetValue<int>("port"),
                    Tags = _config.GetValue<string[]>("tags"),
                    Meta = _config.GetSection("meta").GetChildren()
                        .ToDictionary(x => x.Key, x => x.Value)
                };

                // Set unique ID
                var rand = new Random();
                var instanceId = rand.Next().ToString();
                _serviceRegistration.ID = $"{_serviceRegistration.Name}-{instanceId}";

                // Set hostname
                var dnsHostName = Dns.GetHostName();
                var hostname = await Dns.GetHostEntryAsync(dnsHostName, stoppingToken);
                _serviceRegistration.Address = $"http://{hostname.HostName}";

                logger.LogInformation("Host name set {HostName}", hostname.HostName);

                // Health Check
                _serviceRegistration.Check = new AgentServiceCheck
                {
                    DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(3),
                    Interval = TimeSpan.FromSeconds(30),
                    Timeout = TimeSpan.FromMinutes(1),
                    HTTP = $"{_serviceRegistration.Address}:{_serviceRegistration.Port}/status"
                };

                var result =
                    await consulClient.Agent.ServiceRegister(_serviceRegistration, stoppingToken);

                logger.LogInformation("Consul registration result => {Status}" ,result.StatusCode);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $" Unable to register service");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_serviceRegistration != null)
            {
                logger.LogInformation("Deregistering service");
                await consulClient.Agent.ServiceDeregister(_serviceRegistration.ID,
                    cancellationToken);
            }

            await base.StopAsync(cancellationToken);
        }
    }
}
