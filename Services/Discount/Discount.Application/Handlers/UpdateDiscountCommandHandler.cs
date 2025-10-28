using AutoMapper;
using Discount.Application.Commands;
using Discount.Core.Entities;
using Discount.Core.Repositories;
using Discount.Grpc.Protos;
using MediatR;

namespace Discount.Application.Handlers;

public class UpdateDiscountCommandHandler : IRequestHandler<UpdateDiscountCouponCommand, CouponModel>
{
    private readonly IDiscountRepository _discountRepository;
    private readonly IMapper _mapper;

    public UpdateDiscountCommandHandler(IDiscountRepository repository, IMapper mapper)
    {
        _discountRepository = repository;
        _mapper = mapper;
    }
    public async Task<CouponModel> Handle(UpdateDiscountCouponCommand request, CancellationToken cancellationToken)
    {
        var coupon = _mapper.Map<Coupon>(request);
        var updateSucceeded = await _discountRepository.UpdateDiscount(coupon);
        if (updateSucceeded)
        {
            var updatedCoupon = await _discountRepository.GetDiscount(coupon.ProductName);
            var couponModel = _mapper.Map<CouponModel>(updatedCoupon);
            return couponModel;
        }

        throw new InvalidOperationException("Failed to update discount.");
    }
}