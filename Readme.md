## Messing around with [Yarp](https://github.com/microsoft/reverse-proxy).

### Building and Running
These samples have been built with [.NET 5.0](https://dotnet.microsoft.com/download/dotnet-core?WT.mc_id=dotnet-github-cephilli). Docker config files have been included, but using Docker isn't required for running any of these. The easiest way to run each sample is by navigating into the respective folder in your terminal and execute the following command:
```shell
docker-compose up
```
Similarly, you should be able to run each project in a solution individually with:
```
dotnet run
```

If you're using Visual Studio Code, there are some additional tasks provided for building the samples into Docker containers and running docker-compose.

### Samples
* [Web API Proxying](src/ApiProxySamples) - Routes requests between different Web APIs based on request path and host header.
* [Load Balancing](src/LoadBalancingProxySamples) - Routes requests between two instances of the same service.
* [Code Configuration](src/CodeConfigSample) - Custom IProxyConfigProvider that refreshes routes and clusters configuration using Consul.
