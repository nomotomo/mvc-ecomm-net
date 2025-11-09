using MediatR;

namespace Ordering.Application.Commands;

public class UpdateOrderCommand : IRequest<Guid>
{
    public Guid Id { get; set; }
    public string? UserName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? EmailAddress { get; set; }
    public string? AddressLine { get; set; }
    public string? Country { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public decimal TotalPrice { get; set; }
    public int? PaymentMethod { get; set; }
    public string? CardName { get; set; }
    public string? CardLast4 { get; set; }
    public string? Expiration { get; set; }
    public DateTime? LastModifiedDate { get; set; }
}