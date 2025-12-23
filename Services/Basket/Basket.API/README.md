MediatR is a .NET library that implements the mediator pattern: instead of controllers/api calling services directly, they send commands/queries to a central mediator, which locates and executes the appropriate handler.[1][2]

## What MediatR does here

In this controller, `IMediator` is injected and used to decouple HTTP endpoints from application logic.

- `GetBasketByUserName` creates a `GetBasketByUsernameQuery` and calls `_mediator.Send(query)`, so the controller does not know who or how the basket is fetched.
- `UpdateBasket` posts a `CreateShoppingCartCommand` to `_mediator.Send(...)`, delegating basket creation/update logic.
- `DeleteBasket` and `CheckoutBasket` both build commands/queries and pass them to `_mediator`, keeping the controller thin and framework‑agnostic.

This matches clean architecture: controllers (interface layer) depend only on abstractions (commands/queries) and the mediator, not concrete services or repositories.[2][3][1]

## How commands/queries are handled

Under the hood, each command or query type has a corresponding handler class that implements the appropriate MediatR handler interface.[2]

Typical patterns:

- Queries:
  ```csharp
  public record GetBasketByUsernameQuery(string Username) 
      : IRequest<ShoppingCartResponse>;
  
  public class GetBasketByUsernameQueryHandler 
      : IRequestHandler<GetBasketByUsernameQuery, ShoppingCartResponse>
  {
      public Task<ShoppingCartResponse> Handle(GetBasketByUsernameQuery request, CancellationToken ct)
      {
          // load basket from repository and map to response
      }
  }
  ```
- Commands:
  ```csharp
  public record DeleteBasketByUserNameCommand(string UserName) : IRequest;
  
  public class DeleteBasketByUserNameCommandHandler 
      : IRequestHandler<DeleteBasketByUserNameCommand>
  {
      public async Task<Unit> Handle(DeleteBasketByUserNameCommand request, CancellationToken ct)
      {
          // delete from repository, maybe publish domain events, etc.
          return Unit.Value;
      }
  }
  ```

At startup, you typically register MediatR and scan assemblies for handlers, for example:

```csharp
services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(DeleteBasketByUserNameCommand).Assembly));
```

That registration tells MediatR (and DI) which handlers exist and how to instantiate them.[2]

## What happens when `DeleteBasketByUserNameCommand` is sent

When the controller runs:

```csharp
var cmd = new DeleteBasketByUserNameCommand(basketCheckout.Username);
await _mediator.Send(cmd);
```

the flow is:

1. `_mediator.Send(cmd)` is called in the controller.
2. MediatR inspects the runtime type `DeleteBasketByUserNameCommand`, looks up the matching `IRequestHandler<DeleteBasketByUserNameCommand, TResponse>` in DI.[2]
3. The DI container creates the handler instance, injecting any dependencies (e.g., basket repository, logger).
4. MediatR calls `handler.Handle(cmd, cancellationToken)`.
5. The handler executes the delete logic (e.g., remove the basket from Redis/DB) and returns.
6. `Send` awaits the handler and returns control back to the controller, which then returns `Ok()` or `Accepted()`.

Because the controller only knows about `IMediator` and the command types, you can:

- Change delete logic (e.g., move from Redis to SQL) by changing the handler only.
- Add cross‑cutting behaviors (logging, validation, tracing) as MediatR pipeline behaviors without modifying the controller or handler signatures.[2]

So the command gets “picked up” by handlers through MediatR’s type‑based dispatch plus DI registration: there is exactly one handler per command/query type, and `Send` routes to it automatically.

[1](https://github.com/ChristapherAntony/Clean-Architecture)
[2](https://developers.redhat.com/articles/2023/08/08/implementing-clean-architecture-solutions-practical-example)
[3](https://positiwise.com/blog/clean-architecture-net-core)