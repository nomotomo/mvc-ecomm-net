using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Ordering.Application.Commands;
using Ordering.Application.Mappers;
using Ordering.Core.Entities;
using Ordering.Core.Repositories;

namespace Ordering.Application.Handlers;

public class CheckoutOrderCommandHandler(
    IOrderRepository orderRepository,
    IMapper mapper,
    ILogger<CheckoutOrderCommandHandler> logger)
    : IRequestHandler<CheckoutOrderCommand, Guid>
{

    public async Task<Guid> Handle(CheckoutOrderCommand request, CancellationToken cancellationToken)
    {
        var orderEntity = mapper.Map<Order>(request);
        var newOrder = await orderRepository.AddAsync(orderEntity);
        var outBoxMessage = OrderMapper.MapToOutBoxMessage(newOrder, request.CorrelationId);
        await orderRepository.AddOutBoxMessageAsync(outBoxMessage);
        logger.LogInformation("Order {OrderId} is successfully created with outbox message, " +
                              "with correlation Id: {request.CorrelationId}", newOrder.Id, request.CorrelationId);
        return newOrder.Id;
    }
}