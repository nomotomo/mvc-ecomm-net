namespace Ordering.Core.Common;

public abstract class EntityBase
{
    /// <summary>
    /// Unique identifier for the entity.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Username or identifier of the user who created the entity.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Date and time when the entity was created.
    /// </summary>
    public DateTime CreatedOn { get; set; }

    /// <summary>
    /// Username or identifier of the user who last modified the entity.
    /// </summary>
    public string? LastModifiedBy { get; set; }

    /// <summary>
    /// Date and time when the entity was last modified.
    /// Nullable if never modified.
    /// </summary>
    public DateTime? LastModifiedOn { get; set; }
}