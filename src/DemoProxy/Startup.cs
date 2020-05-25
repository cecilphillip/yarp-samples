using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DemoProxy.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.ReverseProxy.Middleware;

namespace DemoProxy
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddReverseProxy()
                .LoadFromConfig(Configuration.GetSection("ReverseProxy"));
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseMiddleware<HttpInspectorMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapReverseProxy(proxyPipeline =>
                {
                    proxyPipeline.Use((context, next) =>
                    {
                        var lf = proxyPipeline.ApplicationServices.GetRequiredService<ILoggerFactory>();
                        var logger = lf.CreateLogger("Proxy.Middleware");

                        var destinationsFeature = context.Features.Get<IAvailableDestinationsFeature>();

                        logger.LogDebug("Iterating Destinations");
                        foreach (var dest in destinationsFeature.Destinations)
                        {
                            logger.LogDebug($"Destination ID: {dest.DestinationId}\nAddress: {dest.Config.Value.Address}");
                        }

                        return next();
                    });
                    proxyPipeline.UseProxyLoadBalancing();
                });
            });
        }
    }
}