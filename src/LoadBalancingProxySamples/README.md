# Load Balancing Sample

## Projects

| Name       | Role       | Ports                         | Paths      | Notes                       |
|------------|------------|-------------------------------|------------|-----------------------------|
| LBProxy    | Yarp Proxy | 5000 (Host)                   | /items     | Path matching health checks |
| LBItemsApi | Items API  | 5000 (internal), 5001 (local) | /api/items | Health checks               |

## Running the demo

Run demo with docker compose

```shell
docker compose up --build -d
```

Shut down docker compose containers

```shell
docker compose down
```
