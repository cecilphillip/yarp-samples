# API Proxying Sample

## Projects

| Name           | Role        | Ports | Paths              | Notes                       |
|----------------|-------------|-------|--------------------|-----------------------------|
| DemoProxy      | Yarp Proxy  | 5000  | /items, /addresses | Path matching health checks |
| DemoAddressApi | Address API | 5002  | /api/addresses     | Health checks               |
| DemoItemsApi   | Items API   | 5001  | /api/items         | Health checks               |

## Running the demo

Run demo with docker compose

```shell
docker compose up -d 
```

Shut down docker compose containers

```shell
docker compose down 
```
