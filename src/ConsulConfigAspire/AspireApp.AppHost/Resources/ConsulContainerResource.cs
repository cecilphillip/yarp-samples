using System.Net.Sockets;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace AspireApp.AppHost.Resources;

public class ConsulContainerResource(string name) : ContainerResource(name);

public static class DistributedApplicationBuilderExtensions
{
    public static IResourceBuilder<ConsulContainerResource> AddConsulContainer(
        this IDistributedApplicationBuilder builder, string name, int? port = null, string tag = "latest")
    {
        var consul = new ConsulContainerResource(name);
        return builder.AddResource(consul)
            .WithImage("hashicorp/consul", tag)
            .WithImageRegistry("docker.io")
            .WithHttpEndpoint(port,8500, "api" )
            .WithBindMount("./.configs/consul", "/consul/config")
            .WithArgs();
    }
}

