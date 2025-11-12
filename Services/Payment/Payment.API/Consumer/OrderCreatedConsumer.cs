using EventBus.Messages.Common;
using MassTransit;

namespace Payment.API.Consumer;

public class OrderCreatedConsumer(IPublishEndpoint publishEndpoint, ILogger<OrderCreatedConsumer> logger)
    : IConsumer<OrderCreatedEvent>
{
    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var message = context.Message;
        logger.LogInformation("Processing payment for order Id: {OrderId}" +
                              " and correlationId: {CorrelationId}", message.OrderId, message.CorrelationId);
        using (Serilog.Context.LogContext.PushProperty("CorrelationId", message.CorrelationId))
        {
            // Simulate payment processing logic here
            await Task.Delay(2000);
            if (message.TotalPrice > 0)
            {
                var completedEvent = new PaymentCompletedEvent
                {
                    OrderId = message.OrderId,
                    CorrelationId = message.CorrelationId
                };
                await publishEndpoint.Publish(completedEvent);
                logger.LogInformation("Payment completed for order Id: {OrderId}", message.OrderId);
            }
            else
            {
                var failedEvent = new PaymentFailedEvent
                {
                    OrderId = message.OrderId,
                    CorrelationId = message.CorrelationId,
                    Reason = "Invalid payment amount, total price is less than 0."
                };
            
                await publishEndpoint.Publish(failedEvent);
                logger.LogWarning("Payment failed for order Id: {OrderId}", message.OrderId);
            } 
        }
    }
}