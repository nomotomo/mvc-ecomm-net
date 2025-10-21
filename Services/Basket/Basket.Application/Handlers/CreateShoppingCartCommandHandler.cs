using AutoMapper;
using Basket.Application.Commands;
using Basket.Application.Responses;
using Basket.Core.Repositories;
using Basket.Core.Entities;
using MediatR;

namespace Basket.Application.Handlers;

public class CreateShoppingCartCommandHandler : IRequestHandler<CreateShoppingCartCommand, ShoppingCartResponse>
{
    private readonly IBasketRepository _basketRepository;
    private readonly IMapper _mapper;

    public CreateShoppingCartCommandHandler(IBasketRepository basketRepository)
    {
        _basketRepository = basketRepository;
    }
    public async Task<ShoppingCartResponse> Handle(CreateShoppingCartCommand request, CancellationToken cancellationToken)
    {
        //TODO: Will be integrating Discount service here
        var shoppingCart = await _basketRepository.UpdateBasket(new ShoppingCart(request.UserName, request.Items));
        
        var shoppingCartResponse = _mapper.Map<ShoppingCartResponse>(shoppingCart);
        return shoppingCartResponse;
    }
}