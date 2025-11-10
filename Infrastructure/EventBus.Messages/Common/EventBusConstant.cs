namespace EventBus.Messages.Common;

public abstract class EventBusConstant
{
    public const string BasketCheckoutQueue = "BasketCheckoutQueue";
    public const string OrderCreatedQueue = "OrderCreatedQueue";
    public const string PaymentCompletedQueue = "PaymentCompletedQueue";
    public const string PaymentFailedQueue = "PaymentFailedQueue";
}