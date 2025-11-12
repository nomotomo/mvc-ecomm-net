using System.Net;
using AutoMapper;
using Basket.Application.Commands;
using Basket.Application.Queries;
using Basket.Application.Responses;
using Basket.Core.Entities;
using EventBus.Messages.Common;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Basket.API.Controllers;

public class BasketController : ApiController
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private IPublishEndpoint _publishEndpoint;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BasketController(IMediator mediator, IMapper mapper,
        IPublishEndpoint publishEndpoint, IHttpContextAccessor httpContextAccessor)
    {
        _mediator = mediator;
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
        _httpContextAccessor = httpContextAccessor;
    }

    [HttpGet]
    [Route("[action]/{username}", Name = "GetBasketByUserName")]
    [ProducesResponseType(typeof(ShoppingCartResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ShoppingCartResponse>> GetBasketByUserName(string username)
    {
        var query = new GetBasketByUsernameQuery(username);
        var basket = await _mediator.Send(query);
        return Ok(basket);
    }
    
    [HttpPost("CreateBasket")]
    [ProducesResponseType(typeof(ShoppingCartResponse), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<ShoppingCartResponse>> UpdateBasket([FromBody] CreateShoppingCartCommand createShoppingCartCommand)
    {
        var basket = await _mediator.Send(createShoppingCartCommand);
        return Ok(basket);
    }
    
    [HttpDelete]
    [Route("[action]/{userName}", Name = "DeleteBasketByUserName")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<ActionResult> DeleteBasket(string userName) 
    {
        var cmd = new DeleteBasketByUserNameCommand(userName);
        await _mediator.Send(cmd);
        return Ok();
    }
    
    [Route("[action]")]
    [HttpPost]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult> CheckoutBasket([FromBody] BasketCheckout basketCheckout)
    {
        var query = new GetBasketByUsernameQuery(basketCheckout.Username);
        var basket = await _mediator.Send(query);
        if (basket == null) return BadRequest();
        var eventMessage = _mapper.Map<BasketCheckoutEvent>(basketCheckout);
        eventMessage.TotalPrice = basket.TotalPrice;
        var correlationId = _httpContextAccessor.HttpContext?.Request.Headers["x-correlation-id"].FirstOrDefault();
        if (!string.IsNullOrEmpty(correlationId))
        {
            eventMessage.CorrelationId = correlationId;
        }
        else
        {
            eventMessage.CorrelationId = Guid.NewGuid().ToString();
        }
        // send checkout event to rabbitmq
        await _publishEndpoint.Publish(eventMessage);
        // remove the basket
        var cmd = new DeleteBasketByUserNameCommand(basketCheckout.Username);
        await _mediator.Send(cmd);
        return Accepted();
    }
    
}