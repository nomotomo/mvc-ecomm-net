using MediatR;
using Microsoft.Extensions.Logging;
using Ordering.Application.Commands;
using Ordering.Application.Exceptions;
using Ordering.Core.Entities;
using Ordering.Core.Repositories;

namespace Ordering.Application.Handlers;

public class DeleteOrderCommandHandler(IOrderRepository repository, ILogger<DeleteOrderCommandHandler> logger)
    : IRequestHandler<DeleteOrderCommand, Unit>
{
    public async Task<Unit> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdAsync(request.Id);
        if (order == null)
        {
            logger.LogWarning("Order with Id: {OrderId} not found.", request.Id);
            throw new OrderNotFoundException(nameof(Order), request.Id);
        }
        await repository.DeleteAsync(order);
        logger.LogInformation("Order with Id: {OrderId} has been deleted.", request.Id);
        return Unit.Value;
    }
}