using Microsoft.Extensions.Logging;
using Ordering.Core.Entities;

namespace Ordering.Infrastructure.Data;

public class OrderContextSeed
{
    public static async Task SeedAsync(OrderContext orderContext, ILogger<OrderContextSeed> logger)
    {
        if (logger == null) throw new ArgumentNullException(nameof(logger));
        
        if (!orderContext.Orders.Any())
        {
            orderContext.Orders.AddRange(GetOrders());
            await orderContext.SaveChangesAsync();
            logger.LogInformation("Seeded Orders to the database");
        }
    }

    private static IEnumerable<Order> GetOrders()
    {
        return new List<Order>
        {
            new()
            {
                UserName = "rahul",
                FirstName = "Rahul",
                LastName = "Sahay",
                EmailAddress = "rahulsahay@eCommerce.net",
                AddressLine = "Bangalore",
                Country = "India",
                TotalPrice = 750,
                State = "KA",
                ZipCode = "560001",
                CardName = "Visa",
                CardLast4 = "3456",
                CreatedBy = "Rahul",
                Expiration = "12/25",
                PaymentMethod = 1,
                LastModifiedBy = "Rahul",
                LastModifiedOn = DateTime.UtcNow,
                Status = OrderStatus.Shipped
            }
        };
    }
} 
  