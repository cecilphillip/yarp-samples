using Microsoft.Extensions.ServiceDiscovery.Dns;

namespace AspireServiceDiscovery.ServiceDefaults.ServiceEndpointProvider;

public class ConsulDnsSrvServiceEndpointProviderOptions : DnsSrvServiceEndpointProviderOptions
{
    public string DataCenter { get; set; } = string.Empty;
}