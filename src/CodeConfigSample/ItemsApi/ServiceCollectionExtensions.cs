using System.Net.Http.Headers;
using Consul;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ItemsApi;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConsulClient(this IServiceCollection services,
        IConfigurationSection config)
    {
        services.AddHttpClient("Consul", client =>
        {
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        });

        var host = config.GetValue<string>("host") ?? string.Empty;
        var dc = config.GetValue<string>("datacenter") ?? string.Empty;

        var consulClientConfiguration = new ConsulClientConfiguration
        {
            Address = new Uri(host),
            Datacenter = dc
        };

        services.TryAddTransient<IConsulClient>(sp =>
        {
            var clientFactory = sp.GetRequiredService<IHttpClientFactory>();

            return new ConsulClient(consulClientConfiguration,
                clientFactory.CreateClient("Consul"));
        });
        return services;
    }
}
