using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Consul;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.ReverseProxy.Abstractions;
using Microsoft.ReverseProxy.Service;

namespace CCProxy
{
    public class ConsulMonitorWorker : BackgroundService
    {
        private readonly IConsulClient _consulClient;
        private readonly IProxyConfigProvider _proxyConfigProvider;
        private readonly IConfigValidator _proxyConfigValidator;
        private readonly ILogger<ConsulMonitorWorker> _logger;
        private const int DEFAULT_CONSUL_POLL_INTERVAL_MINS = 2;

        public ConsulMonitorWorker(IConsulClient consulClient, IProxyConfigProvider proxyConfigProvider,
            IConfigValidator proxyConfigValidator, ILogger<ConsulMonitorWorker> logger)
        {
            _consulClient = consulClient;
            _proxyConfigProvider = proxyConfigProvider;
            _proxyConfigValidator = proxyConfigValidator;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var serviceResult = await _consulClient.Agent.Services(stoppingToken);

                if (serviceResult.StatusCode == HttpStatusCode.OK)
                {
                    var clusters = await ExtractClusters(serviceResult);
                    var routes = await ExtractRoutes(serviceResult);
                    
                    (_proxyConfigProvider as ConsulProxyConfigProvider)?.Update(routes, clusters);
                }

                await Task.Delay(TimeSpan.FromMinutes(DEFAULT_CONSUL_POLL_INTERVAL_MINS), stoppingToken);
            }
        }

        private async Task<List<Cluster>> ExtractClusters(QueryResult<Dictionary<string, AgentService>> serviceResult)
        {
            var clusters = new Dictionary<string, Cluster>();
            var serviceMapping = serviceResult.Response;
            foreach (var (key, svc) in serviceMapping)
            {
                var cluster = clusters.ContainsKey(svc.Service)
                    ? clusters[svc.Service]
                    : new Cluster {Id = svc.Service};

                cluster.Destinations.Add(svc.ID, new Destination {Address = $"{svc.Address}:{svc.Port}"});

                var clusterErrs = await _proxyConfigValidator.ValidateClusterAsync(cluster);
                if (clusterErrs.Any())
                {
                    _logger.LogError("Errors found when creating clusters for {Service}", svc.Service);
                    clusterErrs.ForEach(err => _logger.LogError(err, $"{svc.Service} cluster validation error"));
                    continue;
                }

                clusters[svc.Service] = cluster;
            }

            return clusters.Values.ToList();
        }

        private async Task<List<ProxyRoute>> ExtractRoutes(QueryResult<Dictionary<string, AgentService>> serviceResult)
        {
            var serviceMapping = serviceResult.Response;
            List<ProxyRoute> routes = new List<ProxyRoute>();
            foreach (var (key, svc) in serviceMapping)
            {
                if (svc.Meta.TryGetValue("yarp", out string enableYarp) &&
                    enableYarp.Equals("on", StringComparison.InvariantCulture))
                {
                    if (routes.Any(r => r.ClusterId == svc.Service)) continue;
                    
                    ProxyRoute route = new ProxyRoute
                    {
                        ClusterId = svc.Service,
                        RouteId = $"{svc.Service}-route",
                        Match =
                        {
                            Path = svc.Meta.ContainsKey("yarp_path")?svc.Meta["yarp_path"] : default,
                            Hosts = svc.Meta.ContainsKey("yarp_hosts")? svc.Meta["yarp_hosts"].Split(',')  : default
                        }
                    };

                    var routeErrs = await _proxyConfigValidator.ValidateRouteAsync(route);
                    if (routeErrs.Any())
                    {
                        _logger.LogError("Errors found when trying to generate routes for {Service}", svc.Service);
                        routeErrs.ForEach(err => _logger.LogError(err, $"{svc.Service} route validation error"));
                        continue;
                    }
                    routes.Add(route);
                }
            }
            return routes;
        }
    }
}