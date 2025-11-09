using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Ordering.Application.Commands;
using Ordering.Application.Exceptions;
using Ordering.Core.Entities;
using Ordering.Core.Repositories;

namespace Ordering.Application.Handlers;

public class UpdateOrderCommandHandler(
    IOrderRepository orderRepository,
    IMapper mapper,
    ILogger<UpdateOrderCommandHandler> logger)
    : IRequestHandler<UpdateOrderCommand, Guid>
{
    public async Task<Guid> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.Id);
        if (order == null)
        {
            throw new OrderNotFoundException(nameof(Order), request.Id);
        }
        mapper.Map(request, order, typeof(UpdateOrderCommand), typeof(Order));
        await orderRepository.UpdateAsync(order);
        logger.LogInformation("Order {OrderId} is successfully updated.", order.Id);
        return order.Id;
    }
}