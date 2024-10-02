using System.Net;
using DnsClient;
using DnsClient.Protocol;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.ServiceDiscovery;

namespace AspireServiceDiscovery.ServiceDefaults.ServiceEndpointProvider;

// taken from https://github.com/dotnet/aspire/blob/main/src/Microsoft.Extensions.ServiceDiscovery.Dns/DnsSrvServiceEndpointProvider.cs
public class ConsulDnsSrvServiceEndpointProvider(
   ServiceEndpointQuery query,
    string srvQuery,
    string hostName,
    IOptionsMonitor<ConsulDnsSrvServiceEndpointProviderOptions> options,
    ILogger<ConsulDnsSrvServiceEndpointProvider> logger,
    IDnsQuery dnsClient,
    TimeProvider timeProvider) : ConsulDnsServiceEndpointProviderBase(query, logger, timeProvider), IHostNameFeature
{
    protected override double RetryBackOffFactor => options.CurrentValue.RetryBackOffFactor;

    protected override TimeSpan MinRetryPeriod => options.CurrentValue.MinRetryPeriod;

    protected override TimeSpan MaxRetryPeriod => options.CurrentValue.MaxRetryPeriod;

    protected override TimeSpan DefaultRefreshPeriod => options.CurrentValue.DefaultRefreshPeriod;

    public override string ToString() => "DNS SRV";

    string IHostNameFeature.HostName => hostName;

    protected override async Task ResolveAsyncCore()
    {
        var endpoints = new List<ServiceEndpoint>();
        var ttl = DefaultRefreshPeriod;
        Log.SrvQuery(logger, ServiceName, srvQuery);
        var result = await dnsClient.QueryAsync(srvQuery, QueryType.SRV, cancellationToken: ShutdownToken).ConfigureAwait(false);
        if (result.HasError)
        {
            throw CreateException(srvQuery, result.ErrorMessage);
        }

        var lookupMapping = new Dictionary<string, DnsResourceRecord>();
        foreach (var record in result.Additionals.Where(x => x is AddressRecord or CNameRecord))
        {
            ttl = MinTtl(record, ttl);
            lookupMapping[record.DomainName] = record;
        }

        var srvRecords = result.Answers.OfType<SrvRecord>();
        foreach (var record in srvRecords)
        {
            if (!lookupMapping.TryGetValue(record.Target, out var targetRecord))
            {
                continue;
            }

            ttl = MinTtl(record, ttl);
            if (targetRecord is AddressRecord addressRecord)
            {
                endpoints.Add(CreateEndpoint(new IPEndPoint(addressRecord.Address, record.Port)));
            }
            else if (targetRecord is CNameRecord canonicalNameRecord)
            {
                endpoints.Add(CreateEndpoint(new DnsEndPoint(canonicalNameRecord.CanonicalName.Value.TrimEnd('.'), record.Port)));
            }
        }

        SetResult(endpoints, ttl);

        static TimeSpan MinTtl(DnsResourceRecord record, TimeSpan existing)
        {
            var candidate = TimeSpan.FromSeconds(record.TimeToLive);
            return candidate < existing ? candidate : existing;
        }

        InvalidOperationException CreateException(string dnsName, string errorMessage)
        {
            var msg = errorMessage switch
            {
                { Length: > 0 } => $"No DNS records were found for service '{ServiceName}' (DNS name: '{dnsName}'): {errorMessage}.",
                _ => $"No DNS records were found for service '{ServiceName}' (DNS name: '{dnsName}')."
            };
            return new InvalidOperationException(msg);
        }

        ServiceEndpoint CreateEndpoint(EndPoint endPoint)
        {
            var serviceEndpoint = ServiceEndpoint.Create(endPoint);
            serviceEndpoint.Features.Set<IServiceEndpointProvider>(this);
            if (options.CurrentValue.ShouldApplyHostNameMetadata(serviceEndpoint))
            {
                serviceEndpoint.Features.Set<IHostNameFeature>(this);
            }

            return serviceEndpoint;
        }
    }
}