using System.Collections.Generic;
using System.Threading;
using Consul;
using Microsoft.Extensions.Primitives;
using Microsoft.ReverseProxy.Abstractions;
using Microsoft.ReverseProxy.Service;

namespace CCProxy
{
    public class ConsulProxyConfigProvider : IProxyConfigProvider
    {
        private volatile ConsulProxyConfig _config;

        public ConsulProxyConfigProvider(IConsulClient consulClient)
        {
            _config = new ConsulProxyConfig(null, null);
        }

        public IProxyConfig GetConfig() => _config;

        public virtual void Update(IReadOnlyList<ProxyRoute> routes, IReadOnlyList<Cluster> clusters)
        {
            var oldConfig = _config;
            _config = new ConsulProxyConfig(routes, clusters);
            oldConfig.SignalChange();
        }

        private class ConsulProxyConfig : IProxyConfig
        {
            private readonly CancellationTokenSource _cts = new CancellationTokenSource();
            public IReadOnlyList<ProxyRoute> Routes { get; }
            public IReadOnlyList<Cluster> Clusters { get; }
            public IChangeToken ChangeToken { get; }

            public ConsulProxyConfig(IReadOnlyList<ProxyRoute> routes, IReadOnlyList<Cluster> clusters)
            {
                Routes = routes;
                Clusters = clusters;
                ChangeToken = new CancellationChangeToken(_cts.Token);
            }

            internal void SignalChange()
            {
                _cts.Cancel();
            }
        }
    }
}