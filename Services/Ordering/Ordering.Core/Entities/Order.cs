using Ordering.Core.Common;

namespace Ordering.Core.Entities;

public class Order : EntityBase
{
    /// <summary>
    /// The username of the customer placing the order.
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// The first name of the customer.
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// The last name of the customer.
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// The email address of the customer.
    /// </summary>
    public string? EmailAddress { get; set; }

    /// <summary>
    /// The address line for delivery.
    /// </summary>
    public string? AddressLine { get; set; }

    /// <summary>
    /// The country of the delivery address.
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// The state or province of the delivery address.
    /// </summary>
    public string? State { get; set; }

    /// <summary>
    /// The zip or postal code of the delivery address.
    /// </summary>
    public string? ZipCode { get; set; }

    /// <summary>
    /// The total price of the order.
    /// </summary>
    public decimal TotalPrice { get; set; }

    /// <summary>
    /// The payment method identifier.
    /// </summary>
    public int? PaymentMethod { get; set; }

    /// <summary>
    /// The name on the payment card.
    /// </summary>
    public string? CardName { get; set; }
    
    /// <summary>
    /// The last 4 digits of the payment card number.
    /// </summary>
    public string? CardLast4 { get; set; }

    /// <summary>
    /// The expiration date of the payment card.
    /// </summary>
    public string? Expiration { get; set; }
    
    /// <summary>
    ///  The current status of the order.
    /// </summary>
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    
}