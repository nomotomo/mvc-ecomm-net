using Discount.Core.Entities;
using MediatR;

namespace Discount.Application.Queries;

public class GetAllDiscountQuery : IRequest<IList<Coupon>>
{
    
}