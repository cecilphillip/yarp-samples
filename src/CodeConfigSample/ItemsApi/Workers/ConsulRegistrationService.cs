using System;
using System.Threading;
using System.Threading.Tasks;
using Consul;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ItemsApi.Workers
{
    public class ConsulRegistrationService : BackgroundService
    {
        private readonly IConsulClient _consulClient;
        private readonly AgentServiceRegistration _serviceRegistration;
        private readonly ILogger<ConsulRegistrationService> _logger;

        public ConsulRegistrationService(IConsulClient consulClient,
            AgentServiceRegistration serviceRegistration, ILogger<ConsulRegistrationService> logger)
        {
            _consulClient = consulClient;
            _serviceRegistration = serviceRegistration;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var result = await _consulClient.Agent.ServiceRegister(_serviceRegistration, stoppingToken);
                _logger.LogInformation(result.StatusCode.ToString());

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $" Unable to register service {_serviceRegistration.Name}");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await _consulClient.Agent.ServiceDeregister(_serviceRegistration.ID, cancellationToken);
            await base.StopAsync(cancellationToken);
        }
    }
}
