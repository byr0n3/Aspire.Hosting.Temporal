# Aspire.Hosting.Temporal

Extensions & helpers for setting up a Temporal resource in Aspire.

## Usage

### Aspire App

```csharp
using Aspire.Hosting;
using Aspire.Hosting.Temporal;

var builder = DistributedApplication.CreateBuilder(args);

// Add the Temporal resource with a PostgreSQL database.
var temporal = builder.AddTemporalContainer(new TemporalContainerOptions
{
	Name = Resources.Temporal,
	Namespace = TemporalConstants.Namespace,
	PostgresName = $"{Resources.Temporal}Postgres",
	DatabaseName = $"{Resources.Temporal}Database",
	DatabaseVolumeName = Resources.TemporalPostgresVolume,
});

// (optional) Add the Temporal UI container resource.
var temporalUi = builder.AddTemporalUi(Resources.TemporalUi, "2.44.1", TemporalConstants.Namespace, temporal);

// Add your worker resource. This could be anything you'd like.
var worker = …;

// Adding a reference to the Temporal resource exposes a ConnectionString you can use.
worker.WithReference(temporal)
      .WaitFor(temporal);

var app = …;

app.WithReference(temporal)
   .WaitFor(temporal);
```

### Docker Compose

You'd (probably) replace most values with environment variables here.

```yaml
services:
  postgres:
    image: "postgres:16-alpine"
    container_name: postgres
    ports:
      - "5432:5432"
    environment:
      - POSTGRES_PORT=5432
      - POSTGRES_USER=temporal
      - POSTGRES_PASSWORD=temporal
    networks:
      - temporal-network
    volumes:
      - temporal-db:/var/lib/postgresql/data
    healthcheck:
      test: [ "CMD-SHELL", "pg_isready -U temporal" ]
      interval: 5s
      timeout: 5s
      retries: 60
      start_period: 30s

  temporal:
    image: ghcr.io/byr0n3/temporal:latest
    container_name: temporal
    depends_on:
      postgres:
        condition: service_healthy
    links:
      - postgres
    ports:
      - "7233:7233"
    environment:
      - DB=postgres12
      - DB_PORT=5432
      - POSTGRES_USER=temporal
      - POSTGRES_PWD=temporal
      - POSTGRES_SEEDS=postgres

      - NAMESPACE=default

      - BIND_ON_IP=0.0.0.0
      - TEMPORAL_BROADCAST_ADDRESS=0.0.0.0
    networks:
      - temporal-network
    healthcheck:
      test: [ "CMD", "nc", "-z", "localhost", "7233" ]
      interval: 5s
      timeout: 3s
      start_period: 30s
      retries: 60

  temporal-ui:
    image: temporalio/ui:2.44.1
    container_name: temporal-ui
    depends_on:
      temporal:
        condition: service_healthy
    ports:
      - "8080:8080"
    environment:
      - TEMPORAL_ADDRESS=temporal:7233
      - TEMPORAL_CORS_ORIGINS=http://localhost:3000
      - TEMPORAL_DEFAULT_NAMESPACE=default
    networks:
      - temporal-network

networks:
  temporal-network:
    name: temporal-network

volumes:
  temporal-db:
```
