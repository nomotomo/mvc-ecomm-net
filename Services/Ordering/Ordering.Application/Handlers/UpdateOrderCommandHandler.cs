using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Ordering.Application.Commands;
using Ordering.Application.Exceptions;
using Ordering.Application.Mappers;
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
        order.MapUpdate(request);
        
        await orderRepository.UpdateAsync(order);
        // optional change: if status change needs to be known by other services, create a different OutBoxMessage type
        var outBoxMessage = OrderMapper.MapToOutBoxMessageForUpdate(order, request.CorrelationId);
        await orderRepository.AddOutBoxMessageAsync(outBoxMessage);
        logger.LogInformation("Order {OrderId} is successfully updated. with correlation id: {request.CorrelationId}", order.Id, request.CorrelationId);
        return order.Id;
    }
}