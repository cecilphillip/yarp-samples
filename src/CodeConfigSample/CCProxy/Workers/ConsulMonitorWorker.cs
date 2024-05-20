using System.Net;
using Consul;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Health;
using Yarp.ReverseProxy.LoadBalancing;
using DestinationConfig = Yarp.ReverseProxy.Configuration.DestinationConfig;
using RouteConfig = Yarp.ReverseProxy.Configuration.RouteConfig;

namespace CCProxy.Workers;

public class ConsulMonitorWorker(
    IConsulClient consulClient,
    IConfigValidator proxyConfigValidator,
    InMemoryConfigProvider proxyConfigProvider,
    ILogger<ConsulMonitorWorker> logger)
    : BackgroundService
{
    // ReSharper disable once InconsistentNaming
    private const int DEFAULT_CONSUL_POLL_INTERVAL_MINS = 2;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Updating route config from Consul...");

            var serviceResult = await consulClient.Agent.Services(stoppingToken);

            if (serviceResult.StatusCode == HttpStatusCode.OK)
            {
                var clusters = await ExtractClusters(serviceResult);
                var routes = await ExtractRoutes(serviceResult);
                proxyConfigProvider.Update(routes, clusters);
            }

            logger.LogInformation("Proxy config reloaded");

            await Task.Delay(TimeSpan.FromMinutes(DEFAULT_CONSUL_POLL_INTERVAL_MINS),
                stoppingToken);
        }
    }

    private async Task<List<ClusterConfig>> ExtractClusters(
        QueryResult<Dictionary<string, AgentService>> serviceResult)
    {
        var clusters = new Dictionary<string, ClusterConfig>();
        var serviceMapping = serviceResult.Response;

        foreach (var (_, svc) in serviceMapping)
        {
            var cluster = clusters.TryGetValue(svc.Service, out var existingCluster)
                ? existingCluster
                : new ClusterConfig
                {
                    ClusterId = svc.Service,
                    LoadBalancingPolicy = LoadBalancingPolicies.RoundRobin,
                    HealthCheck = new()
                    {
                        Active = new ActiveHealthCheckConfig
                        {
                            Enabled = true,
                            Interval = TimeSpan.FromSeconds(10),
                            Timeout = TimeSpan.FromSeconds(10),
                            Policy = HealthCheckConstants.ActivePolicy.ConsecutiveFailures,
                            Path = "/status"
                        }
                    },
                    Metadata = new Dictionary<string, string>
                    {
                        { ConsecutiveFailuresHealthPolicyOptions.ThresholdMetadataName, "5" }
                    }
                };

            var destination = cluster.Destinations is null
                ? new Dictionary<string, DestinationConfig>()
                : new Dictionary<string, DestinationConfig>(cluster.Destinations);

            var address = $"{svc.Address}:{svc.Port}";

            destination.Add(svc.ID,
                new DestinationConfig { Address = address, Health = address });

            var newCluster = cluster with
            {
                Destinations = destination
            };

            var clusterErrs = await proxyConfigValidator.ValidateClusterAsync(newCluster);
            if (clusterErrs.Any())
            {
                logger.LogError("Errors found when creating clusters for {Service}", svc.Service);
                clusterErrs.ForEach(err =>
                    logger.LogError(err, "{Service} cluster validation error", svc.Service));
                continue;
            }

            clusters[svc.Service] = newCluster;
        }

        return clusters.Values.ToList();
    }

    private async Task<List<RouteConfig>> ExtractRoutes(
        QueryResult<Dictionary<string, AgentService>> serviceResult)
    {
        var serviceMapping = serviceResult.Response;
        var routes = new List<RouteConfig>();
        foreach (var (_, svc) in serviceMapping)
        {
            if (!svc.Meta.TryGetValue("yarp", out string? enableYarp) ||
                !enableYarp.Equals("enabled", StringComparison.InvariantCulture)) continue;

            // ignore duplicate service type
            if (routes.Any(r => r.ClusterId == svc.Service)) continue;

            var route = new RouteConfig
            {
                ClusterId = svc.Service,
                RouteId = $"{svc.Service}-route",
                Match = new RouteMatch
                {
                    Path = svc.Meta.TryGetValue("yarp_path", out var yarpPath)
                        ? yarpPath
                        : string.Empty
                },
                Transforms = new IReadOnlyDictionary<string, string>[]
                {
                    new Dictionary<string, string>
                    {
                        ["PathPattern"] =
                            svc.Meta.TryGetValue("yarp_transform_path", out var yarpTransformPath)
                                ? yarpTransformPath
                                : string.Empty
                    }
                }
            };

            var routeErrs = await proxyConfigValidator.ValidateRouteAsync(route);
            if (routeErrs.Any())
            {
                logger.LogError("Errors found when trying to generate routes for {Service}",
                    svc.Service);
                routeErrs.ForEach(err =>
                    logger.LogError(err, "{Service} route validation error", svc.Service));
                continue;
            }

            routes.Add(route);
        }

        return routes;
    }
}
