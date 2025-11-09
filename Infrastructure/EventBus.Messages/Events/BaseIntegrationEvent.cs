namespace EventBus.Messages.Common;

public class BaseIntegrationEvent
{
    public String CorrelationId { get; set; }
    public DateTime CreationDate { get; set; }

    public BaseIntegrationEvent()
    {
        CorrelationId = Guid.NewGuid().ToString();
        CreationDate = DateTime.UtcNow;
    }

    public BaseIntegrationEvent(Guid correlationId, DateTime creationDate)
    {
        CorrelationId = correlationId.ToString();
        CreationDate = creationDate;
    }
}