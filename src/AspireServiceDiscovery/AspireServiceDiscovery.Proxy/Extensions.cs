using System.Net.Http.Headers;
using Consul;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AspireServiceDiscovery.Proxy;

public static class Extensions
{
    public static IReverseProxyBuilder LoadFromConsul(this IReverseProxyBuilder builder)
    {
        //builder.Services.AddHostedService<ConsulMonitorWorker>();

        return builder;
    }
}