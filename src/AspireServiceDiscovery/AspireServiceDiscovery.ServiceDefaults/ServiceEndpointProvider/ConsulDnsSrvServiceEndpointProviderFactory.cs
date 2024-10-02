using System.Diagnostics.CodeAnalysis;
using DnsClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.ServiceDiscovery;

namespace AspireServiceDiscovery.ServiceDefaults.ServiceEndpointProvider;

public class ConsulDnsSrvServiceEndpointProviderFactory(
    IOptionsMonitor<ConsulDnsSrvServiceEndpointProviderOptions> options,
    ILogger<ConsulDnsSrvServiceEndpointProvider> logger,
    IDnsQuery dnsClient,
    TimeProvider timeProvider) : IServiceEndpointProviderFactory
{
    private readonly string _querySuffix = string.IsNullOrEmpty(options.CurrentValue.QuerySuffix)
        ? DefaultDomain : options.CurrentValue.QuerySuffix;
    
    private const string DefaultDomain = "consul";

    private readonly string _dataCenter = options.CurrentValue.DataCenter;
    
    public bool TryCreateProvider(ServiceEndpointQuery query,
        [NotNullWhen(true)] out IServiceEndpointProvider? provider)
    {
        if (string.IsNullOrWhiteSpace(_dataCenter))
        {
            provider = default;
            return false;
        }

        var srvQuery = $"{query.ServiceName}.service.{_dataCenter}.dc.{_querySuffix}";
        provider = new ConsulDnsSrvServiceEndpointProvider(query, srvQuery, hostName: query.ServiceName, options,
            logger, dnsClient, timeProvider);
        return true;
    }
}