using System;
using System.Net;
using System.Text.Json;
using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace ItemsApi
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
            services.AddControllers()
                .AddJsonOptions(option =>
                {
                    option.JsonSerializerOptions.IgnoreNullValues = true;
                    option.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                });
            
            services.AddHealthChecks();
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

            services.AddConsulServiceRegistration(reg =>
            {
                Configuration.Bind("consul:registration", reg);

                if (string.IsNullOrEmpty(reg.ID))
                {
                    var rand = new Random();
                    var instanceId = rand.Next().ToString();
                    reg.ID = $"{reg.Name}-{instanceId}";
                }

                if (string.IsNullOrEmpty(reg.Address))
                {
                    var dnsHostName = Dns.GetHostName();
                    var hostname = Dns.GetHostEntry(dnsHostName);
                    reg.Address = $"http://{hostname.HostName}";
                }

                reg.Check = new AgentServiceCheck
                {
                    DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(2.5),
                    Interval = TimeSpan.FromSeconds(30),
                    Timeout = TimeSpan.FromMinutes(1),
                    HTTP = $"{reg.Address}:{reg.Port}/health"
                };
            });

            services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo {Title = "ItemsApi", Version = "v1"}); });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ItemsApi v1"));


            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health");
                endpoints.MapControllers();
            });
        }
    }
}