using EventBus.Messages.Common;
using MassTransit;
using Ordering.Core.Repositories;

namespace Ordering.API.EventBusConsume;

public class PaymentFailedConsumer(IOrderRepository orderRepository, ILogger<PaymentFailedConsumer> logger)
    : IConsumer<PaymentFailedEvent>
{
    public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
    {
        using var logScope = logger.BeginScope("Consuming PaymentFailedEvent: {eventId}", context.Message.CorrelationId);
        logger.LogInformation("PaymentFailedEvent received: {OrderId}", context.Message.OrderId);

        var orderToUpdate = await orderRepository.GetByIdAsync(context.Message.OrderId);
        if (orderToUpdate == null)
        {
            logger.LogError("Order with Id: {orderId} not found", context.Message.OrderId);
            return;
        }

        orderToUpdate.Status = Core.Entities.OrderStatus.Failed;
        await orderRepository.UpdateAsync(orderToUpdate);

        logger.LogInformation("Order with Id: {orderId} has been updated to Payment Failed", context.Message.OrderId);
    }
}