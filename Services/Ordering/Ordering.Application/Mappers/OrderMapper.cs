using Newtonsoft.Json;
using Ordering.Application.Constants;
using Ordering.Core.Entities;

namespace Ordering.Application.Mappers;

public abstract class OrderMapper
{
    public static OutBoxMessage MapToOutBoxMessage(Order order)
    {
        return new OutBoxMessage
        {
            Type = OutBoxMessageTypes.OrderCreated,
            Content = JsonConvert.SerializeObject(new
            {
                OrderId = order.Id,
                order.UserName,
                order.TotalPrice,
                order.FirstName,
                order.LastName,
                order.EmailAddress,
                order.AddressLine,
                order.Country,
                order.State,
                order.ZipCode,
                order.PaymentMethod,
                order.CardName,
                order.CardLast4,
                order.Expiration,
                order.LastModifiedOn,
                order.Status
            }),
            CorrelationId = Guid.NewGuid().ToString(),
            OccuredOn = DateTime.UtcNow,
        };
    }
}