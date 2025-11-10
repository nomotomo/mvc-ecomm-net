namespace EventBus.Messages.Common;

public class PaymentFailedEvent : BaseIntegrationEvent
{
    public Guid OrderId { get; set; }
    public string UserName { get; set; }
    public string Reason { get; set; }
    public DateTime TimeStamp { get; set; }
}