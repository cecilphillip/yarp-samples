using System;
using System.Text.Json;
using CCProxy.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.ReverseProxy.Service;

namespace CCProxy
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ConsulMonitorWorker>();
            services.AddSingleton<IProxyConfigProvider>(p =>
            {
                return p.GetService<ConsulMonitorWorker>();
            });

            services.AddReverseProxy();

            services.AddHealthChecks();
            services.AddControllers()
                    .AddJsonOptions(option =>
                    {
                        option.JsonSerializerOptions.IgnoreNullValues = true;
                        option.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    });

            services.AddConsulClient(opts =>
            {
                var consulClientConfig = Configuration.GetSection("consul:client");
                var port = consulClientConfig.GetValue<int>("port");
                var host = consulClientConfig.GetValue<string>("host");
                var scheme = consulClientConfig.GetValue<string>("scheme");
                var dc = consulClientConfig.GetValue<string>("datacenter");

                var address = $"{scheme}://{host}:{port}";
                opts.Address = new Uri(address);
                opts.Datacenter = dc;
            });

            services.AddHostedService<ConsulMonitorWorker>(p =>
            {
                return p.GetService<ConsulMonitorWorker>();

            });
        }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health");
                endpoints.MapReverseProxy(pipeline => pipeline.UseMiddleware<HttpInspectorMiddleware>());
                endpoints.MapControllers();
            });
        }
    }
}
