# Messing around with [Yarp](https://github.com/microsoft/reverse-proxy)

## Requirements
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [.NET 8](https://get.dot.net)


## Scenarios
- [Web API Proxying](src/ApiProxySamples) - Routes requests between different Web APIs based on request path.
- [Load Balancing](src/LoadBalancingProxySamples) - Routes requests between two instances of the same service.
- [Code Configuration](src/CodeConfigSample) - Custom IProxyConfigProvider that refreshes routes and clusters configuration using [Consul](https://www.consul.io).
