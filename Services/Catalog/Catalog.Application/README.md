### CQRS with MediatR in .NET

**CQRS** stands for **Command Query Responsibility Segregation**. It’s an architectural pattern that divides how a system handles **read operations (queries)** and **write operations (commands)**. Instead of having one model or service responsible for both, you separate them to achieve clearer design, better scalability, and easier expansion over time.

The **MediatR** library makes applying CQRS in .NET very natural — it acts as an **in-process mediator** that dispatches commands and queries to their respective handlers.

***

### 🧭 Core Principles of CQRS
1. **Commands** modify data — for operations like `CreateProduct`, `UpdateUser`, or `DeleteOrder`.
2. **Queries** read data — for retrieving things like `GetProductById` or `ListOrders`.
3. Each has its own class and handling logic, and they don’t overlap.

This separation helps in:
- **Scalability:** You can scale reads and writes independently.
- **Maintainability:** Each use-case has its own handler; easier to reason about.
- **Flexibility:** You can later swap out the underlying read/write storage or add caching for reads.

***

### ⚙️ How MediatR Fits In
MediatR is a lightweight .NET library that implements the **Mediator pattern**. Instead of making controllers directly call services, everything is routed through **MediatR’s mediator object**.

This improves the *decoupling* between layers — controllers don’t depend on services, only on commands and queries.

***

### 🔧 Typical Implementation Steps

1. **Install MediatR**
   ```bash
   dotnet add package MediatR
   dotnet add package MediatR.Extensions.Microsoft.DependencyInjection
   ```

2. **Register MediatR** in `Program.cs`:
   ```csharp
   builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
   ```

3. **Define a Command and Handler:**
   ```csharp
   // Command
   public record CreateProductCommand(string Name, decimal Price) : IRequest<Guid>;

   // Handler
   public class CreateProductHandler : IRequestHandler<CreateProductCommand, Guid>
   {
       private readonly IProductRepository _repo;
       public CreateProductHandler(IProductRepository repo) => _repo = repo;

       public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
       {
           var id = Guid.NewGuid();
           await _repo.AddAsync(new Product { Id = id, Name = request.Name, Price = request.Price });
           return id;
       }
   }
   ```

4. **Define a Query and Handler:**
   ```csharp
   public record GetProductByIdQuery(Guid Id) : IRequest<Product>;

   public class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, Product>
   {
       private readonly IProductRepository _repo;
       public GetProductByIdHandler(IProductRepository repo) => _repo = repo;

       public async Task<Product> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
           => await _repo.GetByIdAsync(request.Id);
   }
   ```

5. **Use in Controller:**
   ```csharp
   [ApiController]
   [Route("api/products")]
   public class ProductsController : ControllerBase
   {
       private readonly IMediator _mediator;
       public ProductsController(IMediator mediator) => _mediator = mediator;

       [HttpPost]
       public async Task<IActionResult> Create(CreateProductCommand command)
       {
           var id = await _mediator.Send(command);
           return Ok(id);
       }

       [HttpGet("{id}")]
       public async Task<IActionResult> Get(Guid id)
       {
           var product = await _mediator.Send(new GetProductByIdQuery(id));
           return Ok(product);
       }
   }
   ```

***

### 🪄 Why This Pattern is Useful
- **Separates responsibilities:** Each request type has its own focused handler.
- **Improves testability:** Handlers are easy to test in isolation.
- **Simplifies cross-cutting logic:** You can add behaviors (like logging, validation) via MediatR’s pipeline behaviors.

***

### 🧠 Quick Recap
- CQRS divides read and write responsibilities.
- MediatR acts as a mediator between controllers and handlers.
- Commands and Queries are dispatched through `IMediator.Send()`.
- It brings clean architecture and scalability without over-complication.

If you’d like, I can walk you through adding MediatR behaviors like logging, exception handling, or validation next — would you like to try that?

[1](https://www.c-sharpcorner.com/article/cqrs-and-mediatr-pattern-implementation-using-net-core-6-web-api/)
[2](https://code-maze.com/cqrs-mediatr-in-aspnet-core/)
[3](https://codewithmukesh.com/blog/cqrs-and-mediatr-in-aspnet-core/)
[4](https://www.youtube.com/watch?v=BewUyKLZjtc)
[5](https://www.milanjovanovic.tech/blog/cqrs-pattern-with-mediatr)
[6](https://www.c-sharpcorner.com/article/cqrs-pattern-using-mediatr-in-net-5/)
[7](https://www.telerik.com/blogs/applying-cqrs-pattern-aspnet-core-application)
[8](https://www.reddit.com/r/dotnet/comments/1crguen/cqrs_mediatr_is_awesome_net_8/)