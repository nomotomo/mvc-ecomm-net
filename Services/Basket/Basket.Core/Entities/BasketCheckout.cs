namespace Basket.Core.Entities;

public class BasketCheckout
{
    public string Username { get; set; }
    public decimal TotalPrice { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string EmailAddress { get; set; }
    public string AddressLine { get; set; }
    public string Country { get; set; }
    public string State { get; set; }
    public string ZipCode { get; set; }
    public string CardNumber { get; set; }
    public string CardHolderName { get; set; }
    public DateTime CardExpiration { get; set; }
    public string CardSecurityNumber { get; set; }
    public int CardTypeId { get; set; }
}