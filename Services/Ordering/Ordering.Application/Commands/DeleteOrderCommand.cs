using MediatR;

namespace Ordering.Application.Commands;

public class DeleteOrderCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
    public Guid CorrelationId { get; set; }
}