using AutoMapper;
using EventBus.Messages.Common;
using MassTransit;
using MediatR;
using Ordering.Application.Commands;

namespace Ordering.API.EventBusConsume;

public class BasketOrderingConsumer(IMediator mediator, IMapper mapper, ILogger<BasketOrderingConsumer> logger)
    : IConsumer<BasketCheckoutEvent>
{
    public async Task Consume(ConsumeContext<BasketCheckoutEvent> context)
    {
        using var logScope = logger.BeginScope("Consuming BasketCheckoutEvent: {eventId}", context.Message.CorrelationId);
        logger.LogInformation("BasketCheckoutEvent received: {eventId}", context.Message.CorrelationId);
        var command = mapper.Map<CheckoutOrderCommand>(context.Message);
        var result = await mediator.Send(command);
        logger.LogInformation("BasketCheckoutEvent received: {result}", result);
    }
}