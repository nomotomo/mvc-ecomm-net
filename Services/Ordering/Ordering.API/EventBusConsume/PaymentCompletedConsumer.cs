using EventBus.Messages.Common;
using MassTransit;
using Ordering.Core.Repositories;

namespace Ordering.API.EventBusConsume;

public class PaymentCompletedConsumer(IOrderRepository orderRepository, ILogger<PaymentCompletedConsumer> logger)
    : IConsumer<PaymentCompletedEvent>
{
    public async Task Consume(ConsumeContext<PaymentCompletedEvent> context)
    {
        using var logScope = logger.BeginScope("Consuming PaymentCompletedEvent: {eventId}", context.Message.CorrelationId);
        logger.LogInformation("PaymentCompletedEvent received: {OrderId} and correlationId: {CorrelationId}", context.Message.OrderId, context.Message.CorrelationId);

        var orderToUpdate = await orderRepository.GetByIdAsync(context.Message.OrderId);
        if (orderToUpdate == null)
        {
            logger.LogError("Order with Id: {orderId} not found", context.Message.OrderId);
            return;
        }
        
        logger.LogInformation("Setting order status to Paid for OrderId: {orderId}", context.Message.OrderId);
        orderToUpdate.Status = Core.Entities.OrderStatus.Paid;
        await orderRepository.UpdateAsync(orderToUpdate);
        logger.LogInformation("Updated order status in repository for OrderId: {orderId}", context.Message.OrderId);

        logger.LogInformation("Order with Id: {orderId} has been updated to Payment Successful", context.Message.OrderId);
    }
}