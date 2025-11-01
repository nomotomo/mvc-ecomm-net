using Discount.Application.Queries;
using Discount.Core.Entities;
using Discount.Core.Repositories;
using MediatR;

namespace Discount.Application.Handlers;

public class GetAllDiscountQueryHandler : IRequestHandler<GetAllDiscountQuery, IList<Coupon>>
{
    private readonly IDiscountRepository _discountRepository;

    public GetAllDiscountQueryHandler(IDiscountRepository discountRepository)
    {
        _discountRepository = discountRepository;
    }
    public async Task<IList<Coupon>> Handle(GetAllDiscountQuery request, CancellationToken cancellationToken)
    {
        var coupons = await _discountRepository.GetAllDiscounts();
        return coupons;
    }
}