using System;
using Consul;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CCProxy
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddConsulClient(this IServiceCollection services)
        {
            return services.AddConsulClient(options => { });
        }

        public static IServiceCollection AddConsulClient(
            this IServiceCollection services,
            Action<ConsulClientConfiguration> options)
        {
            /*
             * CONSUL_HTTP_ADDR
             * CONSUL_HTTP_SSL
             * CONSUL_HTTP_SSL_VERIFY
             * CONSUL_HTTP_AUTH
             * CONSUL_HTTP_TOKEN
             */
            services.TryAddSingleton<IConsulClient>(sp => new ConsulClient(options));

            return services;
        }
    }
}