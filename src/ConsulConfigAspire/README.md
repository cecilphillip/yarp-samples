# Code Configuration + AspireHost

## Projects

| Name              | Role                  | Ports             | Paths      | Notes                                             |
|-------------------|-----------------------|-------------------|------------|---------------------------------------------------|
| CCProxy           | Yarp Proxy            | 5000              | /items     | Load config from [Consul](https://www.consul.io/) |
| AspireApp.AppHost | Aspire Orchestrator   | 15141 (Dashboard) |            |                                                   |
| Aspire.Extensions | Shared Configurations | 15141 (Dashboard) |            |                                                   |
| ItemsApi          | Items API             | 5010              | /api/items | Health checks, self registered service            |

## Running the demo

Run demo with .NET Aspire

```shell
dotnet run --project AspireApp.AppHost
```
