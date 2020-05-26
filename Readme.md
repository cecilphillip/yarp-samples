## Messing around with [Yarp](https://github.com/microsoft/reverse-proxy).
### Building and Running
All of these require [.NET Core 3.1](https://dotnet.microsoft.com/download?WT.mc_id=dotnet-github-cephilli) or above. Docker config files are included, but using Docker isn't required for running the samples.

If you're using Visual Studio Code, there are some additional tasks provided for building the samples into Docker containiers and running docker-compose.

### Samples
* [Web API Proxying](src/ApiProxySamples) - Routes requests between different Web APIs based on request path and host header.
* [Load Balancing](src/LoadBalancingProxySamples) - Routes requests between two instances of the same service.
