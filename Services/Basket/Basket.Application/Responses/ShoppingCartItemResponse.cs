namespace Basket.Application.Responses;

public class ShoppingCartItemResponse
{
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public string imageFile { get; set; }
    public string productId { get; set; }
    public string productName { get; set; }
}