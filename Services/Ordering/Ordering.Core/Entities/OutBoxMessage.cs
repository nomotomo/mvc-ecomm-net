using Ordering.Core.Common;

namespace Ordering.Core.Entities;

public class OutBoxMessage : EntityBase
{
    public string Type { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public DateTime OccuredOn { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedOn { get; set; } = null;
    public bool isProcessed => ProcessedOn.HasValue;
    public string? ErrorMessage { get; set; } = null;
}