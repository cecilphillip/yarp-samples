# Load Balancing Sample

## Projects

| Name     | Role       | Ports | Paths      | Notes                                             |
|----------|------------|-------|------------|---------------------------------------------------|
| CCProxy  | Yarp Proxy | 5000  | /items     | Load config from [Consul](https://www.consul.io/) |
| ItemsApi | Items API  | 5010  | /api/items | Health checks, self registered service            |

## Running the demo

Run demo with docker compose

```shell
docker compose up --build -d 
```

Shut down docker compose containers

```shell
docker compose down 
```
