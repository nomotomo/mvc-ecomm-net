using Newtonsoft.Json;
using Ordering.Application.Commands;
using Ordering.Application.Constants;
using Ordering.Core.Entities;

namespace Ordering.Application.Mappers;

public static class OrderMapper
{
    public static void MapUpdate(this Order orderToUpdate, UpdateOrderCommand request)
    {
        orderToUpdate.UserName = request.UserName;
        orderToUpdate.TotalPrice = request.TotalPrice;
        orderToUpdate.FirstName = request.FirstName;
        orderToUpdate.LastName = request.LastName;
        orderToUpdate.EmailAddress = request.EmailAddress;
        orderToUpdate.AddressLine = request.AddressLine;
        orderToUpdate.Country = request.Country;
        orderToUpdate.State = request.State;
        orderToUpdate.ZipCode = request.ZipCode;
        orderToUpdate.CardName = request.CardName;
        orderToUpdate.CardLast4 = request.CardLast4;
        orderToUpdate.Expiration = request.Expiration;
        orderToUpdate.PaymentMethod = request.PaymentMethod;
    }
    public static OutBoxMessage MapToOutBoxMessage(Order order, Guid correlationId)
    {
        return new OutBoxMessage
        {
            CorrelationId = correlationId.ToString(),
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
            OccuredOn = DateTime.UtcNow,
        };
    }

    public static OutBoxMessage MapToOutBoxMessageForUpdate(Order orderToUpdate, Guid correlationId)
    {
        return new OutBoxMessage
        {
            CorrelationId = correlationId.ToString(),
            Type = OutBoxMessageTypes.OrderUpdated,
            Content = JsonConvert.SerializeObject(new
            {
                OrderId = orderToUpdate.Id,
                orderToUpdate.UserName,
                orderToUpdate.TotalPrice,
                orderToUpdate.FirstName,
                orderToUpdate.LastName,
                orderToUpdate.EmailAddress,
                orderToUpdate.AddressLine,
                orderToUpdate.Country,
                orderToUpdate.State,
                orderToUpdate.ZipCode,
                orderToUpdate.PaymentMethod,
                orderToUpdate.CardName,
                orderToUpdate.CardLast4,
                orderToUpdate.Expiration,
                orderToUpdate.LastModifiedOn,
                orderToUpdate.Status
            }),
            OccuredOn = DateTime.UtcNow,
        };
    }
}