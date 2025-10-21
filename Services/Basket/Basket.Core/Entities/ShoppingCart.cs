namespace Basket.Core.Entities;

public class ShoppingCart
{
    public string Username { get; set; }
    public List<ShoppingCartItem> Items { get; set; } = new List<ShoppingCartItem>();

    public ShoppingCart()
    {
        
    }
    public ShoppingCart(string username)
    {
        Username = username;
    }

    public ShoppingCart(string username, List<ShoppingCartItem> items)
    {
        Username = username;
        Items = items;
    }
}