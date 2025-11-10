namespace EventBus.Messages.Common;

public class PaymentCompletedEvent : BaseIntegrationEvent
{
    public Guid OrderId { get; set; }
    public string UserName { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime TimeStamp { get; set; }
}