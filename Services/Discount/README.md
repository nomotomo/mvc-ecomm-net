Discount runs as a separate ASP.NET gRPC microservice, and Basket calls it via a generated gRPC client; both are wired and run via the solution plus docker compose so that Basket can fetch discount data over HTTP/2 when applying prices.[1][2][3]

## Discount as gRPC server

From the repo layout, Discount lives under `Services/Discount` and is implemented as an ASP.NET Core gRPC service.  In this pattern:[2][1]

- A `.proto` file in the Discount service defines the **Discount** contract (request/response messages and service methods, e.g., `GetDiscount` / `CreateDiscount`).[3][2]
- ASP.NET Core generates C# server stubs from the proto, and a service class (e.g., `DiscountService : DiscountProtoService.DiscountProtoServiceBase`) overrides the methods to implement logic against the discount database (often PostgreSQL in similar samples).[2][3]
- `Program.cs` / `Startup` configures gRPC in the web host, something like:
    - `builder.Services.AddGrpc();`
    - `app.MapGrpcService<DiscountService>();`
    - `app.MapGet("/", ...);`
- Docker compose exposes the Discount container on a gRPC port (usually 8080/8081) and gives it a DNS name such as `discount.grpc` or `discount.api` for other services.[4][2]

This makes Discount an independent, strongly typed **gRPC server** participating in the microservice system.[3][2]

## Basket as gRPC client

Basket runs as another ASP.NET Core service under `Services/Basket`, and calls Discount via a gRPC client whenever it needs coupon/discount information.  Typical setup in this style of project:[1][2]

- The same `.proto` file is referenced by the Basket project so it gets generated **client** types (e.g., `DiscountProtoService.DiscountProtoServiceClient`).[2][3]
- In Basket `Program.cs`, a typed gRPC client is registered with the DI container, similar to:
  ```csharp
  builder.Services
      .AddGrpcClient<DiscountProtoService.DiscountProtoServiceClient>(o =>
      {
          o.Address = new Uri(configuration["GrpcSettings:DiscountUrl"]);
      });
  ```
  where `GrpcSettings:DiscountUrl` points to the Discount container DNS name and port.[3][2]
- Basket’s application service (e.g., `BasketService` or a discount integration service) injects `DiscountProtoServiceClient` and calls methods like `GetDiscountAsync` when building a shopping cart, then applies `discount.Amount` to the basket’s total.[5][2]

So in clean-architecture terms, **Basket** depends on an abstraction (discount data via gRPC client) rather than HTTP-specific or DB-specific code, and the concrete address is provided by configuration and Docker networking.[6][2]

## Project setup and commands

This repo is designed as a multi-service solution fronted by an API gateway and orchestrated with Docker Compose.  The usual setup and run flow looks like:[1]

### 1. Clone and restore

- Clone the repo:  
  `git clone https://github.com/nomotomo/mvc-ecomm-net.git`  
  `cd mvc-ecomm-net`[1]
- Use the included `global.json` so `dotnet` uses the expected SDK version.[1]
- Restore the solution:  
  `dotnet restore mvc-ecomm-net.sln`[1]

### 2. Local run via Docker Compose

The root contains `compose.yaml` and `docker-compose.override.yml` which define containers for Basket, Discount, Catalog, Ordering, DBs, RabbitMQ, etc.[4][2][1]

Common commands:

- Build and start everything:
  ```bash
  docker compose -f compose.yaml -f docker-compose.override.yml up --build
  ```
  This:
    - Builds service images (including Basket and Discount).
    - Starts infrastructure containers (PostgreSQL, Redis, RabbitMQ) and all microservices.[4][1]

- If using older Docker syntax:
  ```bash
  docker-compose -f compose.yaml -f docker-compose.override.yml up --build
  ```

### 3. Running services directly with dotnet

For development, you can run services individually from the `Services` subfolders.[2][1]

Example sequence:

- Start Discount gRPC service:
  ```bash
  cd Services/Discount/Discount.Grpc
  dotnet run
  ```
- Start Basket service (gRPC client + REST API):
  ```bash
  cd Services/Basket/Basket.API
  dotnet run
  ```

Configuration (appsettings / environment variables or user-secrets) must provide:

- `ConnectionStrings` for Discount DB and Basket storage.[2]
- `GrpcSettings:DiscountUrl` (or similar key) in Basket so the gRPC client knows the Discount endpoint.[3][2]

Once Discount is running on its gRPC port and Basket is configured with its URL, Basket will call Discount via the generated client on each relevant basket operation to fetch and apply discounts.[3][2]