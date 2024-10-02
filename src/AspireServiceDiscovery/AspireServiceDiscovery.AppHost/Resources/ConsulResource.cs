using System.Net.Sockets;

namespace AspireServiceDiscovery.AppHost.Resources;

public class ConsulResource(string name) : ContainerResource(name), IResourceWithServiceDiscovery
{
    internal const string ApiEndpointName = "api";
    internal const int DefaultApiPort = 8500;
    
    internal const string DnsUDPEndpointName = "dns-udp";
    internal const string DnsTCPEndpointName = "dns-tcp";
    internal const int DefaultDnsPort = 8600;

    public string DataCenter { get; set; } = string.Empty;
}

public static class DistributedApplicationBuilderExtensions
{
    /// <summary>
    ///  dig @127.0.0.1 -p 8600  itemsapi.service.yarp-aspire-sd.dc.consul SRV
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="name"></param>
    /// <param name="port"></param>
    /// <param name="dnsPort"></param>
    /// <returns></returns>
    public static IResourceBuilder<ConsulResource> AddConsul(
        this IDistributedApplicationBuilder builder, string name, int? port = null, int? dnsPort = null, string dataCenter = "dc")
    {
        var consul = new ConsulResource(name)
        {
            DataCenter = dataCenter
        };
        
        return builder.AddResource(consul)
            .WithImage("hashicorp/consul")
            .WithImageRegistry("docker.io")
            .WithHttpEndpoint(port ?? ConsulResource.DefaultApiPort,
                ConsulResource.DefaultApiPort,
                ConsulResource.ApiEndpointName)
            .WithEndpoint(ConsulResource.DnsUDPEndpointName, ea =>
            {
                ea.Protocol = ProtocolType.Udp;
                ea.UriScheme = "udp";
                ea.Port = dnsPort ?? ConsulResource.DefaultDnsPort;
                ea.TargetPort = ConsulResource.DefaultDnsPort;
                ea.IsProxied = false;
            })
            .WithEndpoint(ConsulResource.DnsTCPEndpointName, ea =>
            {
                ea.Protocol = ProtocolType.Tcp;
                ea.UriScheme = "tcp";
                ea.Port = dnsPort ?? ConsulResource.DefaultDnsPort;
                ea.TargetPort = ConsulResource.DefaultDnsPort;
                ea.IsProxied = false;
            })
         .WithBindMount("./.config/consul", "/consul/config");
    }
    
    
    public static IResourceBuilder<TDestination> WithReference<TDestination>(
        this IResourceBuilder<TDestination> builder, IResourceBuilder<ConsulResource> source,
        string? rootTokenId = null)
        where TDestination : IResourceWithEnvironment
    {
        builder.WithReference(source as IResourceBuilder<IResourceWithServiceDiscovery>);
        
        return builder.WithEnvironment(ctx =>
        {
            ctx.EnvironmentVariables["CONSUL_DATACENTER"] = source.Resource.DataCenter;
            ctx.EnvironmentVariables["CONSUL_HTTP_ADDR"] = source.Resource.GetEndpoint(ConsulResource.ApiEndpointName);
            
            var dnsEndpoint = source.Resource.GetEndpoint(ConsulResource.DnsUDPEndpointName);
            var dnsAddress = ReferenceExpression.Create(
                $"{dnsEndpoint.Property(EndpointProperty.Host)}:{dnsEndpoint.Property(EndpointProperty.Port)}");
            ctx.EnvironmentVariables["CONSUL_DNS_ADDR"] = dnsAddress;
        });
    }
}