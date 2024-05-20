using System.Net;
using Consul;

namespace ItemsApi.Workers
{
    public class ConsulRegistrationService
        : BackgroundService
    {
        private readonly IConsulClient _consulClient;
        private readonly ILogger<ConsulRegistrationService> _logger;
        private readonly IConfiguration _configRoot;
        private AgentServiceRegistration? _serviceRegistration;

        public ConsulRegistrationService(IConsulClient consulClient,
            IConfiguration configuration,
            ILogger<ConsulRegistrationService> logger)
        {
            _consulClient = consulClient;
            _configRoot = configuration;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var rand = new Random();
            var instanceId = rand.Next().ToString();

            try
            {
                var config = _configRoot.GetSection("consul:registration");
                var boundAddress = _configRoot.GetValue<string>("ASPNETCORE_URLS");

                var port = string.IsNullOrEmpty(boundAddress)
                    ? config.GetValue<int>("port")
                    : new Uri(boundAddress).Port;

                // Set config values
                _serviceRegistration = new AgentServiceRegistration
                {
                    Name = config.GetValue<string>("name"),
                    Port = port,
                    Tags = config.GetValue<string[]>("tags"),
                    Meta = config.GetSection("meta").GetChildren()
                        .ToDictionary(x => x.Key, x => x.Value)
                };

                // Set unique service ID
                _serviceRegistration.ID = $"{_serviceRegistration.Name}-{instanceId}";

                // Set hostname
                var hostName = config.GetValue<string>("host");
                if (string.IsNullOrEmpty(hostName))
                {
                    var dnsHostName = Dns.GetHostName();
                    var host = await Dns.GetHostEntryAsync(dnsHostName, stoppingToken);
                    hostName = host.HostName;
                }

                _serviceRegistration.Address = $"http://{hostName}";

                _logger.LogInformation("Host name set {HostName}", hostName);

                // Health Check
                _serviceRegistration.Check = new AgentServiceCheck
                {
                    CheckID = $"items-api-ttl-{instanceId}",
                    Name = "ItemsAPI TTL Check",
                    Notes ="30s TTL Health Check",
                    Status = HealthStatus.Warning,
                    TTL =  TimeSpan.FromSeconds(30),
                    DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(3)
                };

                var result =
                    await _consulClient.Agent.ServiceRegister(_serviceRegistration, stoppingToken);

                _logger.LogInformation(result.StatusCode.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $" Unable to register service");
            }

            // TTL check report
            while (!stoppingToken.IsCancellationRequested)
            {
                await _consulClient.Agent.PassTTL( $"items-api-ttl-{instanceId}","Good to go 🎉" , stoppingToken);
                await Task.Delay( TimeSpan.FromSeconds(20),
                    stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_serviceRegistration != null)
            {
                _logger.LogInformation("Deregistering service");
                await _consulClient.Agent.ServiceDeregister(_serviceRegistration.ID,
                    cancellationToken);
            }

            await base.StopAsync(cancellationToken);
        }
    }
}
