CorrelationId is used as a **thread‑through ID** so every log coming from Basket → Ordering → Payment for the same checkout shares the same identifier.  That makes it easy to filter and trace one user’s journey in Elasticsearch/Kibana or any log viewer.[1][2][3][4]

***

## What CorrelationId means here

- It is a unique string (often a GUID) attached to each logical operation (for example, “user X checkout at 12:01”).[5]
- That same ID is:
    - Put into HTTP headers when the request hits Basket.
    - Copied into the `BasketCheckoutEvent` message.
    - Passed along into later messages (`OrderCreatedEvent`, `PaymentCompletedEvent`, `PaymentFailedEvent`).
- Logging in Basket, Ordering, and Payment always includes this ID, so you can see the full call chain across microservices.[3][1]

***

## How Basket sets and uses CorrelationId

In `CheckoutBasket`:

```csharp
var httpContext = _httpContextAccessor.HttpContext;
eventMessage.CorrelationId = httpContext?.Items["CorrelationId"] as string
                    ?? httpContext?.Request.Headers["x-correlation-id"].FirstOrDefault()
                    ?? Guid.NewGuid().ToString();
await _publishEndpoint.Publish(eventMessage);
```

Easy explanation:

- When the HTTP request comes in, earlier middleware likely generates a CorrelationId and stores it in `HttpContext.Items["CorrelationId"]` or accepts one from the `x-correlation-id` header.[2][1]
- If neither exists, the controller creates a new GUID so every event has some ID.
- That CorrelationId is put into `BasketCheckoutEvent` before publishing to RabbitMQ.

Result: Basket logs for this request, and any logs produced when handling this event downstream, can all share the same CorrelationId field.[1][3]

***

## How Ordering uses CorrelationId

Ordering touches CorrelationId in two main places:

1. **Consumer of BasketCheckoutEvent**

```csharp
public async Task Consume(ConsumeContext<BasketCheckoutEvent> context)
{
    using var logScope = logger.BeginScope(
        "Consuming BasketCheckoutEvent: {eventId}", context.Message.CorrelationId);
    logger.LogInformation("BasketCheckoutEvent received: {CorrelationId}", context.Message.CorrelationId);
    ...
}
```

- MassTransit delivers the `BasketCheckoutEvent` with the `CorrelationId` property set by Basket.
- The consumer opens a **logging scope** that includes that ID; every log inside the scope automatically has the same CorrelationId.[6][1]
- When Ordering later writes Outbox messages (for `OrderCreatedEvent`), it can also embed the same CorrelationId into the event payload, so the next services see it too.[3]

2. **Outbox dispatcher**

The dispatcher logs message content and type, and when deserializing `OrderCreatedEvent`, it can carry forward the CorrelationId into published events.  This keeps the same ID all the way from Basket → Ordering (create order) → Payment.[7][3]

***

## How Payment uses CorrelationId

In `OrderCreatedConsumer` inside Payment:

```csharp
logger.LogInformation(
    "Processing payment for order Id: {OrderId} and correlationId: {CorrelationId}",
    message.OrderId, message.CorrelationId);

using (Serilog.Context.LogContext.PushProperty("CorrelationId", message.CorrelationId))
{
    // payment logic...
    ...
    await publishEndpoint.Publish(completedEvent);
}
```

Simple explanation:

- Payment receives `OrderCreatedEvent` that already has CorrelationId.
- It logs with that value and uses `LogContext.PushProperty` so **all Serilog logs inside that `using` block automatically include the CorrelationId field**.[8][3]
- When it publishes `PaymentCompletedEvent` or `PaymentFailedEvent`, it copies `message.CorrelationId` into the new event.

Then Ordering’s `PaymentCompletedConsumer` / `PaymentFailedConsumer` again log with that same CorrelationId:

```csharp
using var logScope = logger.BeginScope(
    "Consuming PaymentCompletedEvent: {eventId}", context.Message.CorrelationId);
logger.LogInformation(
    "PaymentCompletedEvent received: {OrderId} and correlationId: {CorrelationId}",
    context.Message.OrderId, context.Message.CorrelationId);
```

So for one checkout, you can query logs by “CorrelationId = X” and see:

- Basket “CheckoutBasket called”
- Ordering “BasketCheckoutEvent received”, “Order created”, “Outbox published OrderCreatedEvent”
- Payment “Processing payment…”, “Payment completed/failed”
- Ordering “PaymentCompletedEvent consumed”, “Order status updated to Paid/Failed”

All connected by that single CorrelationId.[4][1][3]

***

## How to explain it simply

You can phrase it like this:

- “Every incoming request gets a **CorrelationId**, either from the client or generated in middleware. That ID is stored in the HTTP context and added to all log entries in Basket.”[9][2]
- “When Basket publishes a message to RabbitMQ, it puts the same CorrelationId into the event body.”[3]
- “Downstream services (Ordering, Payment) read the event’s CorrelationId, start a logging scope or Serilog `LogContext` with that value, and include it again when they publish new events.”[8][1][3]
- “Because of this, if something goes wrong for one checkout, operations can search logs by CorrelationId and see the entire story across Basket, Ordering, and Payment in Elasticsearch/Kibana.”[4][1][3]

## how ocelot generates a correlation id

Ocelot itself does not magically create your custom `X-Correlation-Id`; either **you add custom middleware** in the gateway, or a library does it for you.[1][2][3]

### 1. What Ocelot supports out of the box

Ocelot has a built‑in *request ID* feature (`RequestIdKey`) that can generate and log a request ID, but that is separate from any custom header convention you use (like `X-Correlation-Id`).[4][1]

In your `ocelot.json` you only have:

```json
"AddHeadersToRequest": {
  "x-correlation-id": "{X-Correlation-Id}"
}
```

This just tells Ocelot:

- “Take whatever value is in the incoming header `X-Correlation-Id` and forward it downstream as `x-correlation-id`.”
- If the client does **not** send that header, Ocelot will simply forward nothing; it does **not** generate a GUID by itself.[5][1]

### 2. How an ID gets generated in practice

There are two common ways this is done:

1. **Custom correlation middleware in the gateway**
    - A small ASP.NET Core middleware runs before Ocelot, checks if `X-Correlation-Id` is present.
    - If missing, it generates `Guid.NewGuid().ToString()` and adds it to the request headers.[2][3]
    - That way, by the time Ocelot evaluates `AddHeadersToRequest`, `{X-Correlation-Id}` always has a value.

   Example pattern (simplified from typical tutorials):[3][2]
   ```csharp
   public class CorrelationIdMiddleware
   {
       private readonly RequestDelegate _next;
       public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

       public async Task Invoke(HttpContext context)
       {
           const string headerName = "X-Correlation-Id";
           if (!context.Request.Headers.TryGetValue(headerName, out var cid) || string.IsNullOrEmpty(cid))
           {
               cid = Guid.NewGuid().ToString();
               context.Request.Headers[headerName] = cid;
           }

           // optional: add to response header or logging context here

           await _next(context);
       }
   }
   ```

   Then register it in Program.cs before `app.UseOcelot()`.

2. **Using a correlation‑ID library in the gateway**
    - Libraries like `SteveGordon/CorrelationId` or similar middleware packages can be plugged into the API Gateway to handle generation and propagation for you.[6][3]
    - Same idea: they ensure the header exists before the request hits Ocelot.

### 3. For your repo specifically

- The `ocelot.json` you showed only configures **propagation**, not **generation**.[7]
- The actual GUID creation likely happens in:
    - A custom middleware class in the ApiGateway project, or
    - A third‑party correlation‑id middleware that the gateway uses.[2][3]

So the correct way to describe it:

> “The gateway runs a correlation‑ID middleware (custom or library) that generates `X-Correlation-Id` when the client does not send it, then Ocelot forwards that header to all downstream services using `AddHeadersToRequest`.”
