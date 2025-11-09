using MediatR;
using Ordering.Application.Responses;

namespace Ordering.Application.Queries;

public class GetOrderListQuery(string userName) : IRequest<List<OrderResponse>>
{
    public readonly string UserName = userName;
}