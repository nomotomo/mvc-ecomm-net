using AutoMapper;
using Basket.Application.Commands;
using Basket.Application.GrpcService;
using Basket.Application.Responses;
using Basket.Core.Repositories;
using Basket.Core.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Basket.Application.Handlers;

public class CreateShoppingCartCommandHandler : IRequestHandler<CreateShoppingCartCommand, ShoppingCartResponse>
{
    private readonly IBasketRepository _basketRepository;
    private readonly IMapper _mapper;
    private readonly DiscountGrpcService _discountGrpcService;
    private ILogger<CreateShoppingCartCommandHandler> logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<CreateShoppingCartCommandHandler>();

    public CreateShoppingCartCommandHandler(IBasketRepository basketRepository, IMapper mapper, DiscountGrpcService discountGrpcService)
    {
        _basketRepository = basketRepository;
        _mapper = mapper;
        _discountGrpcService = discountGrpcService;
    }
    public async Task<ShoppingCartResponse> Handle(CreateShoppingCartCommand request, CancellationToken cancellationToken)
    {
        // List all discounts available

        var coupons = await _discountGrpcService.GetAllDiscounts();
        logger.LogInformation("Getting all available coupons:");
        foreach (var coupon in coupons)
        {
            logger.LogInformation("Available coupon: " + coupon.ProductName + ", Amount: " + coupon.Amount);
        }
        // Call Discount gRPC Service to get discount for each product in the shopping cart
        foreach (var item in request.Items)
        {
            var coupon = await _discountGrpcService.GetDiscount(item.ProductName);
            logger.LogInformation("Coupon amount is: " + coupon.Amount);
            item.Price = Math.Max(0, item.Price - coupon.Amount);
        }
        var shoppingCart = await _basketRepository.UpdateBasket(new ShoppingCart(request.UserName, request.Items));
        
        var shoppingCartResponse = _mapper.Map<ShoppingCartResponse>(shoppingCart);
        return shoppingCartResponse;
    }
}