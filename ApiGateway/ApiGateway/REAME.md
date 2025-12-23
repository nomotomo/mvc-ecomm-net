Ocelot is a lightweight **API gateway** for .NET: the client talks only to Ocelot, and Ocelot forwards the request to the right microservice, adding headers, doing auth, etc.  In this project, Ocelot fronts Catalog, Basket, Ordering and Identity so the Angular UI has a single base URL and consistent cross‑cutting behavior.[1][2][3]

***

## Ocelot in this project (simple view)

Think of Ocelot as a **single front door**:

- The client calls `http://localhost:8010/Catalog/GetAllProducts`.
- Ocelot maps that to `http://catalog.api:8080/api/v1/Catalog/GetAllProducts` using the route entry:
    - `UpstreamPathTemplate` = what client calls.
    - `DownstreamPathTemplate` = real path on the microservice.
    - `DownstreamHostAndPorts` = where that microservice lives (Docker DNS name + port).

Every route in your JSON is one such mapping.[2][1]

Ocelot also forwards the correlation id:

```json
"AddHeadersToRequest": {
  "x-correlation-id": "{X-Correlation-Id}"
}
```

- Whatever `X-Correlation-Id` value exists on the incoming request is added as `x-correlation-id` to the downstream call.[4][1]
- That lets Basket, Ordering and other services log and propagate the same ID through events, which you already use in your logging and SAGA flow.

***

## Where authorization is used in this config

You already have one route with auth:

```json
{
  "DownstreamPathTemplate": "/api/v1/Basket/CheckoutBasket",
  ...
  "UpstreamPathTemplate": "/Basket/Checkout",
  "UpstreamHttpMethod": [ "POST" ],
  "AddHeadersToRequest": {
    "x-correlation-id": "{X-Correlation-Id}"
  },
  "AuthenticationOptions": {
    "AuthenticationProviderKey": "Bearer"
  }
}
```

What this means in simple words:

- This route requires the request to be authenticated using the **“Bearer”** authentication scheme configured in the gateway’s `Program.cs` (for example, JWT bearer tokens).[1]
- If the token is missing or invalid, Ocelot **rejects** the request and never calls `basket.api`.
- Only valid, authorized users reach `/api/v1/Basket/CheckoutBasket` downstream.

Typically, the gateway app configures JWT auth like:

```csharp
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options => { /* Authority, Audience, etc. */ });

app.UseAuthentication();
app.UseOcelot();
```

Then `AuthenticationProviderKey: "Bearer"` ties that route to the configured scheme.[1]

You can protect other routes the same way by adding `AuthenticationOptions` blocks.

***

## How to add rate limiting in Ocelot (concept)

Rate limiting is configured per route or globally in `ocelot.json` via **QoS (Quality of Service) / RateLimitOptions**.[5][1]

Example (not in your snippet, but how you’d do it):

```json
{
  "DownstreamPathTemplate": "/api/v1/Catalog/GetAllProducts",
  "DownstreamScheme": "http",
  "DownstreamHostAndPorts": [{ "Host": "catalog.api", "Port": 8080 }],
  "UpstreamPathTemplate": "/Catalog/GetAllProducts",
  "UpstreamHttpMethod": [ "GET" ],
  "RateLimitOptions": {
    "ClientWhitelist": [],
    "EnableRateLimiting": true,
    "Period": "1s",
    "PeriodTimespan": 1,
    "Limit": 10
  },
  "AddHeadersToRequest": {
    "x-correlation-id": "{X-Correlation-Id}"
  }
}
```

MeaningOcelot is an **API gateway**: a single entry point that receives all client HTTP calls and forwards them to the correct microservice, adding cross‑cutting concerns like auth, headers, and (optionally) rate limiting.[6][1]

***

## Ocelot in simple words

In this project:

- Angular (or any client) talks only to `http://localhost:8010` (the Ocelot gateway), not directly to Catalog, Basket, Ordering, or Identity.[2][1]
- Ocelot reads `ocelot.json` to know:
    - **Upstream**: what the client calls (`/Basket/Checkout`, `/Catalog/GetAllProducts`, `/Order/{userName}`, etc.).
    - **Downstream**: which internal service and path to forward to (`basket.api:8080/api/v1/Basket/CheckoutBasket`, `catalog.api:8080/api/v1/Catalog/GetAllProducts`, and so on).

Example:
- Client calls `GET /Catalog/GetAllProducts` on the gateway.
- Ocelot matches the route and forwards it to `http://catalog.api:8080/api/v1/Catalog/GetAllProducts`.
- It also adds headers like `x-correlation-id` so downstream services can log and trace the request.[4][1]

The same pattern is used for Basket, Ordering, and Identity routes in the JSON you pasted.

---

## How authorization is added in Ocelot

You can see this on the Basket checkout route:

```json
{
  "DownstreamPathTemplate": "/api/v1/Basket/CheckoutBasket",
  ...
  "UpstreamPathTemplate": "/Basket/Checkout",
  "UpstreamHttpMethod": [ "POST" ],
  "AddHeadersToRequest": {
    "x-correlation-id": "{X-Correlation-Id}"
  },
  "AuthenticationOptions": {
    "AuthenticationProviderKey": "Bearer"
  }
}
```

Explanation:

- `"AuthenticationOptions": { "AuthenticationProviderKey": "Bearer" }` tells Ocelot that this route **requires authentication** using the ASP.NET Core auth scheme named `"Bearer"`.[6][1]
- In the ApiGateway project’s Program/Startup, the `Bearer` scheme is usually configured to validate JWT tokens against the Identity service or IdentityServer (authority, audience, signing key, etc.).[6]
- Flow:
    - Client sends `Authorization: Bearer <token>` to the gateway.
    - Ocelot validates the token via the configured handler.
    - If valid, the request is forwarded to the Basket API with user claims; if not, Ocelot returns 401/403 and does **not** call the downstream service.[1][6]

Other routes (like Catalog queries) in your config have no `AuthenticationOptions`, so they remain public/anonymous.

***

## How rate limiting is added (conceptually)

The snippet you showed does not define rate limiting, but Ocelot supports it via configuration per route or globally.[4][1]

Typical JSON block on a route:

```json
"RateLimitRule": {
  "ClientWhitelist": [ "dev-client" ],
  "EnableRateLimiting": true,
  "Period": "1m",
  "PeriodTimespan": 60,
  "Limit": 100
}
```

Meaning in simple words:[7][4]

- `EnableRateLimiting: true`: Turn on rate limiting for this route.
- `Period` / `PeriodTimespan`: Time window (for example, 1 minute).
- `Limit`: Max allowed requests per client in that period (for example, 100 calls/minute).
- `ClientWhitelist`: IDs of clients that are **not** rate limited.

To use this in the project, you would:

- Add `RateLimitRule` to specific routes in `ocelot.json` (for example, on `Basket/Checkout` or high‑cost operations).
- Enable Ocelot’s rate limiting middleware in the ApiGateway project (it reads the config and tracks counts per client/IP or per key).[7][4]

Then Ocelot will automatically block requests that exceed the defined limit before they reach the microservices.

***

So, in one line you can say:

> “Ocelot is the front door: it routes URLs to the right microservice, adds correlation IDs, enforces JWT auth on protected routes, and can apply rate limits so backend services stay protected and simpler.”