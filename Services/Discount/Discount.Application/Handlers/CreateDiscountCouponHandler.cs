using AutoMapper;
using Discount.Application.Commands;
using Discount.Core.Entities;
using Discount.Core.Repositories;
using Discount.Grpc.Protos;
using MediatR;


namespace Discount.Application.Handlers;

public class CreateDiscountCouponHandler : IRequestHandler<CreateDiscountCouponCommand, CouponModel> 
{
    private readonly IMapper _mapper;
    private readonly IDiscountRepository _discountRepository;

    public CreateDiscountCouponHandler(IDiscountRepository repository, IMapper mapper)
    {
        _mapper = mapper;
        _discountRepository = repository;
    }
    public async Task<CouponModel> Handle(CreateDiscountCouponCommand request, CancellationToken cancellationToken)
    {
        var coupon = _mapper.Map<Coupon>(request);
        await _discountRepository.CreateDiscount(coupon);
        var couponModel = _mapper.Map<CouponModel>(coupon);
        return couponModel;
    }
}