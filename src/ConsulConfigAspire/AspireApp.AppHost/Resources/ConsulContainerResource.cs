using System.Net.Sockets;

namespace AspireApp.AppHost.Resources;

public class ConsulContainerResource(string name) : ContainerResource(name);

public static class DistributedApplicationBuilderExtensions
{
    public static IResourceBuilder<ConsulContainerResource> AddConsulContainer(
        this IDistributedApplicationBuilder builder, string name = "consul", int port = 8500, string tag = "latest")
    {
        var consul = new ConsulContainerResource(name);
        return builder.AddResource(consul)
            .WithAnnotation(new VolumeMountAnnotation("./.configs/consul", "/consul/config", VolumeMountType.Bind))
            //.WithAnnotation(new VolumeMountAnnotation("./.tmp/consul", "/opt/consul", VolumeMountType.Bind, true))
            .WithAnnotation(new ServiceBindingAnnotation(ProtocolType.Tcp, uriScheme: "http", port: port, containerPort: 8500))
            .WithAnnotation(new ContainerImageAnnotation { Image = "hashicorp/consul", Tag = tag });
    }
}

