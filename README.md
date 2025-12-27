# E-Commerce Microservices Platform

A cloud-native, microservices-based e-commerce platform built with **.NET 8**, **ASP.NET Core**, **Angular**, and deployed on **Kubernetes**. The system demonstrates modern architectural patterns including **Clean Architecture**, **CQRS**, **Event-Driven Architecture**, and **API Gateway** patterns.

---

## Table of Contents

1. [Overview](#overview)
2. [Architecture](#architecture)
3. [Core Services](#core-services)
4. [Technology Stack](#technology-stack)
5. [Design Patterns](#design-patterns)
6. [Communication Patterns](#communication-patterns)
7. [Data Persistence](#data-persistence)
8. [API Gateway (Ocelot)](#api-gateway-ocelot)
9. [Observable Systems](#observable-systems)
10. [Local Development Setup](#local-development-setup)
11. [Deployment](#deployment)
12. [Q&A](#qa)

---

## Overview

This project is a **modern microservices architecture** for an e-commerce platform that separates business concerns into independently deployable services. Each service owns its data, can be scaled individually, and communicates with other services via REST, gRPC, or asynchronous messaging.

### Key Features

- ✅ **Microservices Architecture** – Independent services per business domain
- ✅ **Clean Architecture** – Separation of concerns (Core, Application, Infrastructure layers)
- ✅ **CQRS Pattern** – Separate read and write operations using MediatR
- ✅ **Event-Driven** – Asynchronous communication via RabbitMQ/MassTransit
- ✅ **gRPC** – Efficient synchronous inter-service communication
- ✅ **API Gateway** – Ocelot routes and centralizes client requests
- ✅ **Distributed Tracing** – CorrelationId for end-to-end request tracking
- ✅ **Containerized** – Docker & Kubernetes ready
- ✅ **Observability** – Structured logging, Elasticsearch, Kibana integration

---

## Architecture

### High-Level System Design

```
┌─────────────────────────────────────────────────────────────────┐
│                     Angular Frontend (Port 4200)                 │
│                                                                   │
└────────────────────────┬────────────────────────────────────────┘
                         │ REST/JSON
                         ▼
        ┌────────────────────────────────────┐
        │   Ocelot API Gateway (Port 8010)   │
        │  • Routing                         │
        │  • Auth/Rate Limiting              │
        │  • Cross-cutting concerns          │
        └────────────────────────────────────┘
                    │    │    │    │
        ┌───────────┼────┼────┼────┼───────────┐
        │           │    │    │    │           │
        ▼           ▼    ▼    ▼    ▼           ▼
    ┌────────┐ ┌──────┐ ┌────────┐ ┌────────┐ ┌───────┐
    │Catalog │ │Basket│ │Ordering│ │Payment │ │Discount
    │Service │ │Service│Service  │ │Service │ │Service│
    │(Mongo) │ │(Redis)│(Postgres)│(Postgres)│(Postgres)
    └────────┘ └──────┘ └────────┘ └────────┘ └───────┘
        │           │        │        │          │
        └───────────┴────────┴────────┴──────────┘
                    │
        ┌───────────▼──────────────┐
        │  RabbitMQ Message Broker │
        │  (Event-Driven Workflow) │
        └────────────────────────────┘
                    │
        ┌───────────▼──────────────┐
        │   Elasticsearch/Kibana   │
        │   (Logging & Monitoring) │
        └────────────────────────────┘
```

### Service Interaction Flow

```
User Checkout Flow:
─────────────────

1. User adds products → Basket Service (Redis)
2. User clicks "Checkout" → Basket publishes BasketCheckoutEvent
3. Ordering Service consumes event → Creates Order
4. Ordering publishes OrderCreatedEvent
5. Payment Service consumes → Processes payment
6. Payment publishes PaymentCompletedEvent or PaymentFailedEvent
7. Ordering consumes payment event → Updates order status
8. Frontend notified of order completion
```

---

## Core Services

### 1. **Catalog Service** (Product Management)

**Purpose:** Manages product information, brands, types, and inventory.

**Tech Stack:**
- Database: **MongoDB** (flexible schema for product metadata)
- API: REST endpoints
- Pattern: Clean Architecture with Repository pattern

**Key Endpoints:**
```
GET    /api/v1/Catalog/GetAllProducts     # List products with pagination
GET    /api/v1/Catalog/GetProductById/{id}
GET    /api/v1/Catalog/GetAllBrands
GET    /api/v1/Catalog/GetAllTypes
GET    /api/v1/Catalog/GetProductByBrandName/{brand}
POST   /api/v1/Catalog/CreateProduct
PUT    /api/v1/Catalog/UpdateProduct
```

**Why MongoDB?**
- Product schemas vary (shoes, balls, rackets have different attributes)
- Document store allows flexible JSON structures
- Easy horizontal scaling for catalog reads

**Key Classes:**
```csharp
Product         // Entity: id, name, description, imageFile, price, brand, type
Brand           // Entity: id, name
Type            // Entity: id, name
IProductRepository  // Abstraction for data access
GetAllProductsQuery // CQRS query
GetAllProductsQueryHandler // Handles pagination, filtering, sorting
```

---

### 2. **Basket Service** (Shopping Cart Management)

**Purpose:** Manages user shopping carts with real-time discount integration.

**Tech Stack:**
- Database: **Redis** (in-memory cache, perfect for carts)
- External Communication: **gRPC to Discount Service**
- Messaging: **RabbitMQ/MassTransit** for checkout events
- Pattern: CQRS with event publishing

**Key Endpoints:**
```
GET    /api/v1/Basket/GetBasketByUserName/{userName}
POST   /api/v1/Basket/CreateBasket
POST   /api/v1/Basket/CheckoutBasket
DELETE /api/v1/Basket/{userName}
```

**Why Redis?**
- Baskets are temporary, frequently accessed (session-like)
- Key-value model matches "basket per user" perfectly
- Sub-millisecond latency for read/write
- TTL support for automatic cleanup

**Key Classes:**
```csharp
Basket              // Model: userName, items[], totalPrice
BasketItem          // Contains: productId, productName, price, quantity
BasketCheckoutEvent // Published event for checkout workflow
CheckoutBasketCommand  // CQRS command
CheckoutBasketCommandHandler // Handles checkout logic, publishes event
```

**Discount Integration (gRPC):**
- When checkout happens, calls Discount service via gRPC
- Applies discounts in real-time
- Reduces basket total before publishing checkout event

---

### 3. **Ordering Service** (Order Management & Fulfillment)

**Purpose:** Manages order lifecycle from creation to fulfillment.

**Tech Stack:**
- Database: **PostgreSQL** (transactional ACID compliance needed)
- ORM: **Entity Framework Core** (migrations, relationships)
- Messaging: **RabbitMQ/MassTransit** (consume checkout, publish order events)
- Persistence: **Outbox Pattern** (ensure events are published reliably)
- Pattern: Clean Architecture, CQRS, SAGA pattern

**Key Endpoints:**
```
GET    /api/v1/Order/{userName}         # Get user's orders
POST   /api/v1/Order                    # Create order (from checkout)
PUT    /api/v1/Order                    # Update order
DELETE /api/v1/Order/{id}
```

**Key Classes:**
```csharp
Order               // Entity: id, userName, items[], status, totalPrice, createdDate
OrderItem           // Contains: productId, productName, price, quantity
OrderStatus         // Enum: Pending, Paid, Shipped, Delivered
BasketCheckoutEvent // Consumed from Basket
OrderCreatedEvent   // Published after order creation
PaymentCompletedEvent  // Consumed from Payment service
PaymentFailedEvent     // Consumed from Payment service

// Outbox pattern
OutboxMessage       // Stores events before publishing to RabbitMQ
// Guarantees: Order creation & event publishing are atomic
```

**Order Workflow (SAGA Pattern):**
1. Consumes `BasketCheckoutEvent` from Basket
2. Creates Order in DB with status = "Pending"
3. Writes `OrderCreatedEvent` to Outbox table (same transaction)
4. Background worker reads Outbox, publishes `OrderCreatedEvent` to RabbitMQ
5. Consumes `PaymentCompletedEvent` from Payment service
6. Updates order status to "Paid"
7. Consumes `PaymentFailedEvent` if payment fails
8. Updates order status to "Failed" and notifies user

---

### 4. **Payment Service** (Payment Processing)

**Purpose:** Processes payments and emits completion/failure events.

**Tech Stack:**
- Database: **PostgreSQL** (stores payment records)
- ORM: **Entity Framework Core**
- Messaging: **RabbitMQ/MassTransit**
- Pattern: CQRS, event consumer/publisher

**Key Endpoints:**
```
POST   /api/v1/Payment/ProcessPayment
GET    /api/v1/Payment/{orderId}
```

**Key Classes:**
```csharp
Payment             // Entity: id, orderId, amount, status, paymentDate
PaymentStatus       // Enum: Pending, Completed, Failed
OrderCreatedEvent   // Consumed event (triggers payment)
PaymentCompletedEvent  // Published on success
PaymentFailedEvent     // Published on failure
```

**Payment Workflow:**
1. Consumes `OrderCreatedEvent` with order details
2. "Processes" payment (simulated or real API call)
3. Updates Payment record in DB
4. Publishes either:
   - `PaymentCompletedEvent` (success) → Ordering updates order status
   - `PaymentFailedEvent` (failure) → Ordering marks order as failed

---

### 5. **Discount Service** (Coupon & Discount Management)

**Purpose:** Manages coupons and discount rules, consulted by Basket via gRPC.

**Tech Stack:**
- Database: **PostgreSQL**
- ORM: **Entity Framework Core**
- gRPC: Exposes `GetDiscount` method for Basket service

**Key Endpoints (REST):**
```
GET    /api/v1/Discount/{discountCode}
POST   /api/v1/Discount/CreateDiscount
PUT    /api/v1/Discount/UpdateDiscount
DELETE /api/v1/Discount/{id}
```

**gRPC Service Definition (discount.proto):**
```protobuf
service DiscountService {
  rpc GetDiscount(GetDiscountRequest) returns (GetDiscountReply);
}

message GetDiscountRequest {
  string couponCode = 1;
}

message GetDiscountReply {
  double discountAmount = 1;
  double discountPercentage = 2;
}
```

**Key Classes:**
```csharp
Discount            // Entity: id, couponCode, description, discountPercentage, amount
GetDiscountRequest  // gRPC request
GetDiscountReply    // gRPC response
```

---

## Technology Stack

### Backend

| Layer | Technology | Purpose |
|-------|-----------|---------|
| **API Gateway** | Ocelot | Route, authenticate, rate-limit external requests |
| **Web Framework** | ASP.NET Core 8 | REST API, gRPC host |
| **Business Logic** | MediatR | CQRS implementation, dependency injection |
| **Databases** | PostgreSQL, MongoDB, Redis | Persistent storage per service |
| **ORM** | Entity Framework Core | Data access, migrations (SQL services) |
| **Message Broker** | RabbitMQ | Asynchronous event publishing/subscribing |
| **Message Pattern** | MassTransit | RabbitMQ integration, consumer/publisher abstractions |
| **RPC Framework** | gRPC + Protobuf | High-performance inter-service calls (Basket ↔ Discount) |
| **Observability** | Serilog, Elasticsearch, Kibana | Structured logging, distributed tracing |
| **Containerization** | Docker | Build reproducible images |
| **Orchestration** | Kubernetes | Deploy, scale, manage containers |

### Frontend

| Technology | Purpose |
|-----------|---------|
| **Angular 18+** | SPA framework, modern signals |
| **TypeScript** | Type-safe scripting |
| **RxJS** | Reactive programming for HTTP calls |
| **Angular Router** | Client-side routing |
| **HttpClientModule** | REST API communication via gateway |

### Development Tools

- **Visual Studio 2022** / **VS Code** – IDE
- **Docker Desktop** – Local containerization
- **Docker Compose** – Multi-container orchestration (local dev)
- **Postman/Swagger** – API testing

---

## Design Patterns

### 1. **Clean Architecture**

Each service follows a 4-layer structure:

```
Service/
├── Core/
│   ├── Entities/          # Domain models (e.g., Product, Order)
│   ├── Specifications/    # Business rules, filtering logic
│   ├── Repositories/      # Repository interfaces (abstractions)
│   └── Events/            # Domain events
│
├── Application/
│   ├── Commands/          # CQRS commands (write operations)
│   ├── Queries/           # CQRS queries (read operations)
│   ├── Handlers/          # MediatR command/query handlers
│   ├── Consumers/         # Message consumers (RabbitMQ)
│   ├── DTOs/              # Data transfer objects
│   └── Mappings/          # Entity-to-DTO mappings
│
├── Infrastructure/
│   ├── Data/              # DbContext, EF Core migrations
│   ├── Repositories/      # Concrete repository implementations
│   ├── Services/          # External service integrations (gRPC clients)
│   ├── Messaging/         # RabbitMQ publisher/consumer setup
│   └── Caching/           # Redis integration
│
└── API/
    ├── Controllers/       # REST endpoints
    ├── Extensions/        # DI, middleware setup
    ├── Middlewares/       # Cross-cutting (logging, correlation)
    └── appsettings.json  # Configuration
```

**Benefits:**
- Core domain logic is independent of frameworks
- Easy testing (inject mock repositories)
- Infrastructure changes don't affect business logic
- Clear separation of concerns

---

### 2. **CQRS (Command Query Responsibility Segregation)**

**Principle:** Separate read operations (queries) from write operations (commands).

**Example – Basket Checkout:**

```csharp
// WRITE OPERATION (Command)
public record CheckoutBasketCommand(string UserName) : IRequest<Unit>;

public class CheckoutBasketCommandHandler : IRequestHandler<CheckoutBasketCommand>
{
    public async Task<Unit> Handle(CheckoutBasketCommand request, CancellationToken cancellationToken)
    {
        var basket = await _basketRepository.GetBasketAsync(request.UserName);
        
        // Apply business logic
        var discountedTotal = await _discountService.ApplyDiscountAsync(basket);
        
        // Publish event
        var checkoutEvent = new BasketCheckoutEvent
        {
            UserName = basket.UserName,
            Items = basket.Items,
            TotalPrice = discountedTotal,
            CorrelationId = GetCorrelationId()
        };
        await _publishEndpoint.Publish(checkoutEvent);
        
        // Clear basket
        await _basketRepository.DeleteBasketAsync(request.UserName);
        
        return Unit.Value;
    }
}

// READ OPERATION (Query)
public record GetBasketQuery(string UserName) : IRequest<BasketDto>;

public class GetBasketQueryHandler : IRequestHandler<GetBasketQuery, BasketDto>
{
    public async Task<BasketDto> Handle(GetBasketQuery request, CancellationToken cancellationToken)
    {
        var basket = await _basketRepository.GetBasketAsync(request.UserName);
        return _mapper.Map<BasketDto>(basket);
    }
}
```

**Benefits:**
- Optimize reads separately (can be cached, denormalized)
- Write logic is explicit and testable
- Scalability: scale read replicas independently

---

### 3. **Repository Pattern**

Abstracts data access, allowing tests to inject mocks.

```csharp
// Interface (Core layer)
public interface IBasketRepository
{
    Task<Basket> GetBasketAsync(string userName);
    Task<Basket> UpdateBasketAsync(Basket basket);
    Task DeleteBasketAsync(string userName);
}

// Implementation (Infrastructure layer)
public class BasketRepository : IBasketRepository
{
    private readonly IConnectionMultiplexer _redis;
    
    public async Task<Basket> GetBasketAsync(string userName)
    {
        var data = await _redis.GetDatabase().StringGetAsync($"basket:{userName}");
        return data.IsNullOrEmpty ? null : JsonConvert.DeserializeObject<Basket>(data);
    }
    
    public async Task<Basket> UpdateBasketAsync(Basket basket)
    {
        var json = JsonConvert.SerializeObject(basket);
        await _redis.GetDatabase().StringSetAsync($"basket:{basket.UserName}", json, TimeSpan.FromHours(24));
        return basket;
    }
    
    public async Task DeleteBasketAsync(string userName)
    {
        await _redis.GetDatabase().KeyDeleteAsync($"basket:{userName}");
    }
}
```

**Benefits:**
- Test by injecting mock repositories
- Swap Redis for another cache without changing business logic
- Centralized data access

---

### 4. **Event-Driven Architecture (SAGA Pattern)**

Services communicate asynchronously via events, coordinating multi-step workflows.

```csharp
// Step 1: Basket publishes checkout
public class CheckoutBasketCommandHandler : IRequestHandler<CheckoutBasketCommand>
{
    public async Task<Unit> Handle(CheckoutBasketCommand request, ...)
    {
        var checkoutEvent = new BasketCheckoutEvent { ... };
        await _publishEndpoint.Publish(checkoutEvent);
        return Unit.Value;
    }
}

// Step 2: Ordering consumes checkout
public class BasketCheckoutConsumer : IConsumer<BasketCheckoutEvent>
{
    public async Task Consume(ConsumeContext<BasketCheckoutEvent> context)
    {
        var order = new Order
        {
            UserName = context.Message.UserName,
            Items = context.Message.Items,
            Status = OrderStatus.Pending
        };
        await _orderRepository.CreateOrderAsync(order);
        
        // Write to Outbox (same transaction)
        await _outboxRepository.WriteOutboxAsync(new OrderCreatedEvent { OrderId = order.Id });
    }
}

// Step 3: Outbox dispatcher publishes events
public class OutboxDispatcher : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var messages = await _outboxRepository.GetPendingMessagesAsync();
            foreach (var msg in messages)
            {
                await _publishEndpoint.Publish(msg);
                await _outboxRepository.MarkAsPublishedAsync(msg.Id);
            }
            await Task.Delay(5000, stoppingToken);
        }
    }
}

// Step 4: Payment consumes order
public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
{
    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var payment = ProcessPayment(context.Message);
        await _paymentRepository.SaveAsync(payment);
        
        if (payment.IsSuccessful)
            await _publishEndpoint.Publish(new PaymentCompletedEvent { OrderId = context.Message.OrderId });
        else
            await _publishEndpoint.Publish(new PaymentFailedEvent { OrderId = context.Message.OrderId });
    }
}

// Step 5: Ordering consumes payment result
public class PaymentCompletedConsumer : IConsumer<PaymentCompletedEvent>
{
    public async Task Consume(ConsumeContext<PaymentCompletedEvent> context)
    {
        var order = await _orderRepository.GetOrderAsync(context.Message.OrderId);
        order.Status = OrderStatus.Paid;
        await _orderRepository.UpdateOrderAsync(order);
    }
}
```

**Outbox Pattern:**
Ensures order creation and event publishing are atomic. If service crashes after creating order but before publishing event, the Outbox dispatcher will replay it.

```csharp
// In OrderCreatedConsumer
using (var transaction = await _context.Database.BeginTransactionAsync())
{
    order = await _context.Orders.AddAsync(new Order { ... });
    await _context.SaveChangesAsync();  // Persist order
    
    // Same transaction: insert into Outbox
    await _context.OutboxMessages.AddAsync(new OutboxMessage 
    { 
        EventId = Guid.NewGuid(),
        EventType = nameof(OrderCreatedEvent),
        EventData = JsonConvert.SerializeObject(new OrderCreatedEvent { ... }),
        IsProcessed = false
    });
    await _context.SaveChangesAsync();
    
    await transaction.CommitAsync();  // Atomic commit
}
```

**Benefits:**
- Loose coupling: services don't call each other directly
- Scalability: process events asynchronously
- Resilience: failures don't block the whole flow
- Observability: trace events via CorrelationId

---

## Communication Patterns

### 1. **Synchronous: REST + HTTP**

**Used for:**
- Client ↔ API Gateway (external requests)
- Service ↔ Service for immediate results (not ideal but sometimes necessary)

**Example:**
```csharp
// Ocelot routes to Catalog service
GET http://localhost:8010/Catalog/GetAllProducts
→ Routes to: http://catalog.api:8080/api/v1/Catalog/GetAllProducts
```

---

### 2. **Synchronous: gRPC**

**Used for:**
- Basket ↔ Discount (real-time discount lookup)

**Why gRPC over REST:**
- Strongly typed contracts (.proto files)
- Smaller payloads (binary protobuf vs JSON)
- HTTP/2 multiplexing
- Faster serialization/deserialization

**Example (discount.proto):**
```protobuf
syntax = "proto3";

service DiscountService {
  rpc GetDiscount (GetDiscountRequest) returns (GetDiscountReply);
}

message GetDiscountRequest {
  string couponCode = 1;
}

message GetDiscountReply {
  double discountAmount = 1;
  double discountPercentage = 2;
}
```

**Basket calls Discount via gRPC:**
```csharp
public class CheckoutBasketCommandHandler : IRequestHandler<CheckoutBasketCommand>
{
    private readonly DiscountGrpcClient _discountClient;
    
    public async Task<Unit> Handle(CheckoutBasketCommand request, ...)
    {
        var basket = await _basketRepository.GetBasketAsync(request.UserName);
        
        // Call Discount service via gRPC
        var discountReply = await _discountClient.GetDiscountAsync(new GetDiscountRequest 
        { 
            CouponCode = basket.CouponCode 
        });
        
        basket.TotalPrice -= discountReply.DiscountAmount;
        
        // Publish checkout event
        await _publishEndpoint.Publish(new BasketCheckoutEvent { ... });
        
        return Unit.Value;
    }
}
```

---

### 3. **Asynchronous: RabbitMQ + MassTransit**

**Used for:**
- Basket → Ordering (checkout workflow)
- Ordering → Payment (payment processing)
- Payment → Ordering (payment result)

**Event Flow:**

```
BasketCheckoutEvent
├─ Consumed by: Ordering.BasketCheckoutConsumer
│  └─ Creates Order, publishes OrderCreatedEvent
│
OrderCreatedEvent
├─ Consumed by: Payment.OrderCreatedConsumer
│  └─ Processes payment, publishes PaymentCompletedEvent or PaymentFailedEvent
│
PaymentCompletedEvent / PaymentFailedEvent
└─ Consumed by: Ordering.PaymentCompletedConsumer / PaymentFailedConsumer
   └─ Updates order status
```

**MassTransit Configuration:**
```csharp
// In Ordering service startup
services.AddMassTransit(x =>
{
    // Register consumers
    x.AddConsumer<BasketCheckoutConsumer>();
    x.AddConsumer<PaymentCompletedConsumer>();
    x.AddConsumer<PaymentFailedConsumer>();
    
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq.service", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
        
        cfg.ConfigureEndpoints(context);
    });
});

// Publish event
public class CheckoutBasketCommandHandler : IRequestHandler<CheckoutBasketCommand>
{
    private readonly IPublishEndpoint _publishEndpoint;
    
    public async Task<Unit> Handle(CheckoutBasketCommand request, ...)
    {
        var @event = new BasketCheckoutEvent { ... };
        await _publishEndpoint.Publish(@event);
        return Unit.Value;
    }
}
```

---

## Data Persistence

### Why Polyglot Persistence?

Different services pick the database that best fits their workload:

| Service | Database | Why |
|---------|----------|-----|
| **Catalog** | MongoDB | Flexible schema (products have different attributes) |
| **Basket** | Redis | In-memory, fast reads/writes, session-like data |
| **Ordering** | PostgreSQL | ACID transactions, relational data, audit trail |
| **Payment** | PostgreSQL | Transaction guarantees, audit requirements |
| **Discount** | PostgreSQL | Simple relational data, strong consistency |

### Entity Framework Core (Ordering & Payment)

```csharp
// DbContext
public class OrderingContext : DbContext
{
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>()
            .HasKey(o => o.Id);
        
        modelBuilder.Entity<Order>()
            .HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey("OrderId");
        
        // Configure Outbox for reliability
        modelBuilder.Entity<OutboxMessage>()
            .HasKey(o => o.EventId);
    }
}

// Migration
dotnet ef migrations add InitialCreate --project Infrastructure --startup-project API
dotnet ef database update --project Infrastructure --startup-project API
```

### MongoDB (Catalog)

```csharp
// MongoDB context
public class CatalogContext : MongoDBContext
{
    public IMongoCollection<Product> Products { get; set; }
    public IMongoCollection<Brand> Brands { get; set; }
    public IMongoCollection<Type> Types { get; set; }
    
    public CatalogContext(IMongoClient mongoClient, string databaseName) 
        : base(mongoClient, databaseName)
    {
    }
}

// Repository
public class ProductRepository : IProductRepository
{
    private readonly IMongoCollection<Product> _products;
    
    public async Task<IEnumerable<Product>> GetAllProductsAsync(
        int pageIndex = 0, int pageSize = 10, string brandId = null, string typeId = null)
    {
        var filterBuilder = Builders<Product>.Filter;
        var filters = new List<FilterDefinition<Product>>();
        
        if (!string.IsNullOrEmpty(brandId))
            filters.Add(filterBuilder.Eq(p => p.BrandId, brandId));
        
        if (!string.IsNullOrEmpty(typeId))
            filters.Add(filterBuilder.Eq(p => p.TypeId, typeId));
        
        var filter = filters.Count > 0 ? filterBuilder.And(filters) : filterBuilder.Empty;
        
        return await _products
            .Find(filter)
            .Skip(pageIndex * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }
}
```

### Redis (Basket)

```csharp
// Redis repository
public class BasketRepository : IBasketRepository
{
    private readonly IConnectionMultiplexer _redis;
    private const string BASKET_PREFIX = "basket:";
    
    public async Task<Basket> GetBasketAsync(string userName)
    {
        var data = await _redis.GetDatabase().StringGetAsync($"{BASKET_PREFIX}{userName}");
        return data.IsNullOrEmpty ? null : JsonConvert.DeserializeObject<Basket>(data);
    }
    
    public async Task<Basket> UpdateBasketAsync(Basket basket)
    {
        var json = JsonConvert.SerializeObject(basket);
        var expiryTime = TimeSpan.FromHours(24);  // Basket expires after 24 hours
        await _redis.GetDatabase().StringSetAsync($"{BASKET_PREFIX}{basket.UserName}", json, expiryTime);
        return basket;
    }
    
    public async Task DeleteBasketAsync(string userName)
    {
        await _redis.GetDatabase().KeyDeleteAsync($"{BASKET_PREFIX}{userName}");
    }
}
```

---

## API Gateway (Ocelot)

### Purpose

Routes client requests to appropriate backend services, centralizing:
- Request routing
- Authentication/Authorization
- Rate limiting
- Request correlation (CorrelationId)
- Cross-cutting concerns

### Configuration (ocelot.json)

```json
{
  "Routes": [
    {
      "DownstreamPathTemplate": "/api/v1/Catalog/GetAllProducts",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        { "Host": "catalog.api", "Port": 8080 }
      ],
      "UpstreamPathTemplate": "/Catalog/GetAllProducts",
      "UpstreamHttpMethod": [ "GET" ],
      "AddHeadersToRequest": {
        "x-correlation-id": "{X-Correlation-Id}"
      }
    },
    {
      "DownstreamPathTemplate": "/api/v1/Basket/GetBasketByUserName/{userName}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        { "Host": "basket.api", "Port": 8080 }
      ],
      "UpstreamPathTemplate": "/Basket/{userName}",
      "UpstreamHttpMethod": [ "GET" ]
    },
    {
      "DownstreamPathTemplate": "/api/v1/Basket/CheckoutBasket",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        { "Host": "basket.api", "Port": 8080 }
      ],
      "UpstreamPathTemplate": "/Basket/Checkout",
      "UpstreamHttpMethod": [ "POST" ],
      "AuthenticationOptions": {
        "AuthenticationProviderKey": "Bearer"
      }
    },
    {
      "DownstreamPathTemplate": "/api/v1/Order/{userName}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        { "Host": "ordering.api", "Port": 8080 }
      ],
      "UpstreamPathTemplate": "/Order/{userName}",
      "UpstreamHttpMethod": [ "GET" ]
    }
  ],
  "GlobalConfiguration": {
    "BaseUrl": "http://localhost:8010"
  }
}
```

### Middleware for Correlation ID

```csharp
// In API Gateway middleware
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    
    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers["x-correlation-id"].FirstOrDefault() 
                            ?? Guid.NewGuid().ToString();
        
        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers.Add("x-correlation-id", correlationId);
        
        // Log the request with correlation ID
        using (_logger.BeginScope(new Dictionary<string, object> 
        { 
            { "CorrelationId", correlationId } 
        }))
        {
            await _next(context);
        }
    }
}
```

---

## Observable Systems

### Correlation ID (Distributed Tracing)

**What is CorrelationId?**
A unique identifier attached to each logical operation (e.g., one user's checkout) that flows through all services and logs.

**How It Works:**

1. **API Gateway generates CorrelationId:**
```csharp
var correlationId = context.Request.Headers["x-correlation-id"].FirstOrDefault() 
                    ?? Guid.NewGuid().ToString();
context.Items["CorrelationId"] = correlationId;
```

2. **Basket embeds it in the event:**
```csharp
public class CheckoutBasketCommandHandler : IRequestHandler<CheckoutBasketCommand>
{
    public async Task<Unit> Handle(CheckoutBasketCommand request, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var correlationId = httpContext?.Items["CorrelationId"] as string 
                           ?? Guid.NewGuid().ToString();
        
        var @event = new BasketCheckoutEvent
        {
            UserName = request.UserName,
            Items = basket.Items,
            CorrelationId = correlationId  // ← Embedded in event
        };
        
        await _publishEndpoint.Publish(@event);
        return Unit.Value;
    }
}
```

3. **Ordering receives it and logs with it:**
```csharp
public class BasketCheckoutConsumer : IConsumer<BasketCheckoutEvent>
{
    public async Task Consume(ConsumeContext<BasketCheckoutEvent> context)
    {
        using (_logger.BeginScope("CorrelationId", context.Message.CorrelationId))
        {
            _logger.LogInformation("Consuming BasketCheckoutEvent");
            
            var order = new Order { ... };
            await _orderRepository.CreateOrderAsync(order);
            
            var orderEvent = new OrderCreatedEvent
            {
                OrderId = order.Id,
                CorrelationId = context.Message.CorrelationId  // ← Forwarded
            };
            await _publishEndpoint.Publish(orderEvent);
        }
    }
}
```

4. **Payment continues the chain:**
```csharp
public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
{
    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        using (LogContext.PushProperty("CorrelationId", context.Message.CorrelationId))
        {
            _logger.LogInformation("Processing payment for order {OrderId}", context.Message.OrderId);
            
            var payment = ProcessPayment(...);
            await _paymentRepository.SaveAsync(payment);
            
            var completedEvent = new PaymentCompletedEvent
            {
                OrderId = context.Message.OrderId,
                CorrelationId = context.Message.CorrelationId  // ← Forwarded
            };
            await _publishEndpoint.Publish(completedEvent);
        }
    }
}
```

**Result:**
All logs from Basket → Ordering → Payment for one checkout share the same CorrelationId, allowing tracing in Elasticsearch/Kibana:

```
correlationId: 12345abc

[Basket] CheckoutBasket called - correlationId: 12345abc
[Ordering] BasketCheckoutEvent received - correlationId: 12345abc
[Ordering] Order created - correlationId: 12345abc
[Payment] Processing payment - correlationId: 12345abc
[Payment] Payment completed - correlationId: 12345abc
[Ordering] Order status updated to Paid - correlationId: 12345abc
```

### Structured Logging (Serilog)

```csharp
// In appsettings.json
{
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      {
        "Name": "Elasticsearch",
        "Args": {
          "nodeUris": "http://elasticsearch:9200",
          "indexFormat": "logs-{0:yyyy.MM.dd}"
        }
      }
    ],
    "Properties": {
      "Application": "Ordering.Service",
      "Environment": "Production"
    }
  }
}

// In Program.cs
builder.Host.UseSerilog((context, config) =>
{
    config
        .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://elasticsearch:9200"))
        {
            IndexFormat = "logs-{0:yyyy.MM.dd}",
            AutoRegisterTemplate = true
        })
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "Ordering")
        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName);
});
```

### Kibana Dashboards

Query by CorrelationId to see entire flow:
```
CorrelationId: 12345abc
```

Dashboard shows:
- Timeline of all events
- Services involved
- Any errors or warnings
- Performance metrics

---

## Local Development Setup

### Prerequisites

- **.NET 8 SDK**
- **Docker & Docker Compose**
- **Node.js 18+** (for Angular)
- **PostgreSQL** (optional, Docker handles it)
- **MongoDB** (optional, Docker handles it)
- **Redis** (optional, Docker handles it)
- **RabbitMQ** (optional, Docker handles it)

### Quick Start

1. **Clone the repository:**
```bash
git clone https://github.com/nomotomo/mvc-ecomm-net.git
cd mvc-ecomm-net
```

2. **Start infrastructure (Docker Compose):**
```bash
docker-compose -f docker-compose.yml -f docker-compose.override.yml up -d
```

This starts:
- PostgreSQL (Ordering, Payment, Discount)
- MongoDB (Catalog)
- Redis (Basket)
- RabbitMQ (Message broker)
- Elasticsearch (Logging)
- Kibana (Log visualization)

3. **Build and run backend services:**
```bash
# In separate terminals or use a task runner

# Gateway
cd Gateway.API
dotnet run

# Catalog
cd Services/Catalog/Catalog.API
dotnet run

# Basket
cd Services/Basket/Basket.API
dotnet run

# Ordering
cd Services/Ordering/Ordering.API
dotnet run

# Payment
cd Services/Payment/Payment.API
dotnet run

# Discount
cd Services/Discount/Discount.API
dotnet run
```

4. **Build and run frontend:**
```bash
cd Web/ClientApp
npm install
ng serve
```

5. **Access the application:**
- **Frontend:** http://localhost:4200
- **API Gateway:** http://localhost:8010
- **Kibana:** http://localhost:5601
- **RabbitMQ Management:** http://localhost:15672 (guest/guest)

### Docker Compose Services

```yaml
services:
  # Infrastructure
  postgres:
    image: postgres:15
    environment:
      POSTGRES_PASSWORD: Password@123
    ports:
      - "5432:5432"

  mongodb:
    image: mongo:6
    ports:
      - "27017:27017"

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"

  rabbitmq:
    image: rabbitmq:3-management
    ports:
      - "5672:5672"
      - "15672:15672"

  elasticsearch:
    image: docker.elastic.co/elasticsearch/elasticsearch:8.5.0
    environment:
      - discovery.type=single-node
      - xpack.security.enabled=false
    ports:
      - "9200:9200"

  kibana:
    image: docker.elastic.co/kibana/kibana:8.5.0
    ports:
      - "5601:5601"
```

---

## Deployment

### Docker Build & Push

```bash
# Build image for each service
docker build -t myregistry/catalog-api:latest ./Services/Catalog/Catalog.API
docker build -t myregistry/basket-api:latest ./Services/Basket/Basket.API
docker build -t myregistry/ordering-api:latest ./Services/Ordering/Ordering.API
docker build -t myregistry/payment-api:latest ./Services/Payment/Payment.API
docker build -t myregistry/discount-api:latest ./Services/Discount/Discount.API
docker build -t myregistry/gateway:latest ./Gateway.API

# Push to registry
docker push myregistry/catalog-api:latest
# ... repeat for other services
```

### Kubernetes Deployment

1. **Create ConfigMaps for configuration:**
```bash
kubectl create configmap catalog-config --from-literal=ConnectionString="mongodb://mongodb:27017"
kubectl create configmap ordering-config --from-literal=ConnectionString="postgresql://user:password@postgres:5432/ordering"
```

2. **Apply Kubernetes manifests:**
```bash
kubectl apply -f k8s/catalog-deployment.yaml
kubectl apply -f k8s/basket-deployment.yaml
kubectl apply -f k8s/ordering-deployment.yaml
kubectl apply -f k8s/payment-deployment.yaml
kubectl apply -f k8s/discount-deployment.yaml
kubectl apply -f k8s/gateway-deployment.yaml
```

3. **Example Kubernetes manifest (catalog-deployment.yaml):**
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: catalog-api
spec:
  replicas: 3
  selector:
    matchLabels:
      app: catalog-api
  template:
    metadata:
      labels:
        app: catalog-api
    spec:
      containers:
      - name: catalog-api
        image: myregistry/catalog-api:latest
        ports:
        - containerPort: 8080
        env:
        - name: MONGODB_CONNECTIONSTRING
          valueFrom:
            configMapKeyRef:
              name: catalog-config
              key: ConnectionString
        - name: RABBITMQ_HOST
          value: "rabbitmq-service"
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        livenessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /ready
            port: 8080
          initialDelaySeconds: 10
          periodSeconds: 5

---
apiVersion: v1
kind: Service
metadata:
  name: catalog-service
spec:
  selector:
    app: catalog-api
  ports:
  - port: 8080
    targetPort: 8080
  type: ClusterIP
```

4. **Monitor deployment:**
```bash
kubectl get deployments
kubectl get services
kubectl logs -f deployment/catalog-api

# Scale a service
kubectl scale deployment catalog-api --replicas=5

# Rolling update
kubectl set image deployment/catalog-api catalog-api=myregistry/catalog-api:v2
```

---

## Q&A

### Architecture & Microservices

**Q: Why microservices?**
A: Allows each business domain (Catalog, Basket, Ordering, Payment, Discount) to be developed, deployed, and scaled independently. Reduces coupling, enables faster iteration, and supports targeted scaling (e.g., scale Basket during peak traffic without scaling Catalog).

**Q: How do services communicate?**
A: Two patterns:
1. **Synchronous (gRPC):** Basket → Discount for real-time discount lookup (low latency, strongly typed)
2. **Asynchronous (RabbitMQ/MassTransit):** Basket → Ordering → Payment for checkout workflow (loose coupling, resilience)

**Q: How are service boundaries defined?**
A: By business domains. Each service owns one domain's data and logic:
- Catalog owns products
- Basket owns shopping carts (Redis)
- Ordering owns order lifecycle
- Payment owns payment processing
- Discount owns coupons
Each service has its own database for schema autonomy.

---

### Clean Architecture & CQRS

**Q: What is Clean Architecture here?**
A: Each service is layered:
- **Core:** Domain entities, abstractions
- **Application:** CQRS commands/queries, MediatR handlers, use cases
- **Infrastructure:** EF Core, repositories, external integrations
- **API:** Controllers, middleware
Benefits: Framework-agnostic logic, testability, maintainability.

**Q: Why CQRS?**
A: E-commerce has read-heavy (product browsing) and write-heavy (checkout) flows. CQRS separates them:
- **Commands** enforce business rules (checkout validation)
- **Queries** optimize reads (caching, denormalization)
Allows scaling reads independently.

**Q: How is dependency inversion used?**
A: Controllers depend on MediatR and abstractions, not directly on repositories. Repositories are interfaces in Core, implemented in Infrastructure. DI injects concrete implementations, keeping business logic framework-agnostic.

---

### Basket, Redis & Discount

**Q: Why Redis for Basket?**
A: Baskets are temporary, session-like, accessed frequently with simple key-value operations. Redis provides millisecond latency, fits the "basket per user" pattern, and reduces database load.

**Q: How does Basket use Discount via gRPC?**
A: Basket generates a gRPC client from `.proto` file. When checkout happens, it calls `GetDiscount` method on Discount service, applies discount in real-time, and adjusts basket total.

**Q: Why gRPC over REST?**
A: gRPC offers strongly typed contracts, smaller payloads (binary protobuf), HTTP/2 multiplexing, and better performance for internal service-to-service calls.

---

### RabbitMQ, MassTransit & SAGA

**Q: How is checkout implemented as a SAGA?**
A:
1. Basket publishes `BasketCheckoutEvent`
2. Ordering consumes it, creates Order, writes to Outbox, publishes `OrderCreatedEvent`
3. Payment consumes, processes, publishes `PaymentCompletedEvent`
4. Ordering consumes, updates order status
CorrelationId threads through all steps for tracing.

**Q: What problem does the Outbox pattern solve?**
A: Avoids inconsistency where order is saved but event is not published. Order creation and Outbox insert happen in one transaction; a background worker reliably publishes events from Outbox to RabbitMQ.

**Q: How do consumers achieve idempotency?**
A: Update operations guard against re-applying same event by checking order status or maintaining a message log.

---

### Databases & Persistence

**Q: Why polyglot persistence?**
A: Different workloads need different databases:
- PostgreSQL for transactional data (ACID)
- MongoDB for flexible schemas
- Redis for fast caching
This allows optimal performance per service.

**Q: How does Entity Framework fit in?**
A: EF Core handles ORM, migrations, and relationships in SQL services. Repositories abstract it, allowing business logic to stay framework-agnostic.

---

### Observability

**Q: What is CorrelationId?**
A: A unique ID attached to each logical operation (e.g., one user's checkout). It flows through all services (Basket → Ordering → Payment) and embeds in every log, allowing tracing the entire flow.

**Q: How is it used?**
A:
- API Gateway generates or accepts from header
- Basket embeds in `BasketCheckoutEvent`
- Ordering logs with it, forwards in `OrderCreatedEvent`
- Payment logs with it, forwards in completion event
Query logs by CorrelationId in Kibana to see the full story.

---

### Deployment

**Q: How to run locally?**
A: Docker Compose brings up all infrastructure (Postgres, MongoDB, Redis, RabbitMQ, Elasticsearch). Run each service with `dotnet run` or containerize and use Docker Compose for services too.

**Q: How to deploy to Kubernetes?**
A: Build images for each service, push to registry, apply Kubernetes Deployments/Services. ConfigMaps/Secrets handle configuration. Rolling updates enable zero-downtime deployments.

**Q: How is configuration managed?**
A: Externalized via appsettings, environment variables, or Kubernetes ConfigMaps/Secrets. Keeps images immutable; same image promotes dev → staging → production with different configs.

---

## Key Takeaways

1. **Microservices:** Separate services per business domain for independent scaling and deployment
2. **Clean Architecture:** Layered approach with clear separation of concerns
3. **CQRS:** Separate read and write operations for scalability
4. **Event-Driven:** Asynchronous communication via RabbitMQ for loose coupling
5. **gRPC:** Fast, typed inter-service calls where synchronous is needed
6. **Polyglot Persistence:** Databases chosen per workload (Postgres, MongoDB, Redis)
7. **CorrelationId:** Distributed tracing across services for observability
8. **Outbox Pattern:** Reliable event publishing with transactional guarantees
9. **API Gateway:** Centralized routing and cross-cutting concerns
10. **Containerization & Orchestration:** Docker and Kubernetes for deployment

---

## Getting Help

- **Issues:** GitHub Issues
- **Documentation:** See `/docs` folder
- **Architecture Diagram:** See `/architecture` folder
- **API Documentation:** Swagger at each service's `/swagger` endpoint

---

## License

This project is open source under the MIT License. See LICENSE file for details.

---

## Contributors

Built as a demonstration of modern microservices architecture patterns and best practices for cloud-native applications.

**Author:** Saurabh Mishra/Nomotomo
**Last Updated:** December 2025
