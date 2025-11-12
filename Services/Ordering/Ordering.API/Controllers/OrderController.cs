using MediatR;
using Microsoft.AspNetCore.Mvc;
using Ordering.Application.Commands;
using Ordering.Application.Queries;
using Ordering.Application.Responses;

namespace Ordering.API.Controllers;

public class OrderController : ApiController
{
    private const string CorrelationIdHeader = "x-correlation-id";
    private readonly IMediator _mediator;
    private readonly ILogger<OrderController> _logger;

    public OrderController(IMediator mediator, ILogger<OrderController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }
    
    [HttpGet("{userName}", Name = "GetOrdersByUserName")]
    [ProducesResponseType(typeof(IEnumerable<OrderResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<OrderResponse>>> GetOrdersByUserName(string userName)
    {
        _logger.LogInformation("Getting orders for user: {UserName}", userName);
        // Implementation to get orders by username
        var query = new GetOrderListQuery(userName);
        var orders = await _mediator.Send(query);
        return Ok(orders); // Placeholder response
    }
    
    [HttpPost(Name = "CreateOrder")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    public async Task<ActionResult<Guid>> CheckoutOrder([FromBody] CheckoutOrderCommand command)
    {
        //extract correlation id x-correlation-id
        var correlationId = HttpContext.Request.Headers[CorrelationIdHeader].FirstOrDefault() ?? Guid.NewGuid().ToString();
        command.CorrelationId = Guid.Parse(correlationId);
        var result = await _mediator.Send(command);
        _logger.LogInformation("Order created with Id: {result} with correlationId {correlationId}", result, correlationId);
        return Ok(result);
    }

    [HttpPut(Name = "UpdateOrder")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Guid>> UpdateOrder([FromBody] UpdateOrderCommand command)
    {
        //extract correlation id x-correlation-id
        var correlationId = HttpContext.Request.Headers[CorrelationIdHeader].FirstOrDefault() ?? Guid.NewGuid().ToString();
        command.CorrelationId = Guid.Parse(correlationId);
        var result = await _mediator.Send(command);
        _logger.LogInformation("Order updated with Id: {result} with correlationId {correlationId}", result, correlationId);
        return Ok(result);
    }

    [HttpDelete("{id}", Name = "DeleteOrder")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteOrder(Guid id)
    {
        //extract correlation id x-correlation-id
        var correlationId = HttpContext.Request.Headers[CorrelationIdHeader].FirstOrDefault() ?? Guid.NewGuid().ToString();
        var cmd = new DeleteOrderCommand { Id = id , CorrelationId = Guid.Parse(correlationId) };
        var result = await _mediator.Send(cmd);
        _logger.LogInformation("Order deleted with Id: {result} with correlationId {correlationId}", result, correlationId);
        return NoContent();
    }
}