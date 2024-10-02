using System.Net;
using DnsClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ServiceDiscovery;
using Microsoft.Extensions.ServiceDiscovery.Dns;

namespace AspireServiceDiscovery.ServiceDefaults.ServiceEndpointProvider;

public static class ConsulDnsSrvServiceEndpointProviderExtensions
{
    public static IServiceCollection AddConsulDnsSrvServiceEndpointProvider(this IServiceCollection services, string consulAddress, Action<ConsulDnsSrvServiceEndpointProviderOptions> configureOptions)
    {
        services.AddServiceDiscoveryCore();
        services.AddSingleton<IDnsQuery>(_ =>
        {
            var consulUri = new Uri(consulAddress);
            var endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), consulUri.Port);
            return new LookupClient(endPoint);
        });
        
        services.AddSingleton<IServiceEndpointProviderFactory, ConsulDnsSrvServiceEndpointProviderFactory>();
        var options = services.AddOptions<ConsulDnsSrvServiceEndpointProviderOptions>();
        options.Configure(o => configureOptions?.Invoke(o));
        return services;
    }
}