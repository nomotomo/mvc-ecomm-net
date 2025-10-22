using AutoMapper;
using Basket.Application.Queries;
using Basket.Application.Responses;
using Basket.Core.Repositories;
using MediatR;

namespace Basket.Application.Handlers;

public class GetBasketByUsernameHandler : IRequestHandler<GetBasketByUsernameQuery, ShoppingCartResponse>
{
    private readonly IBasketRepository _basketRepository;
    private readonly IMapper _mapper;

    public GetBasketByUsernameHandler(IBasketRepository basketRepository, IMapper mapper)
    {
        _basketRepository = basketRepository;
        _mapper = mapper;
    }
    public async Task<ShoppingCartResponse> Handle(GetBasketByUsernameQuery request, CancellationToken cancellationToken)
    {
        var shoppingCart = await _basketRepository.GetBasket(request.Username);
        var shoppingCartResponse = _mapper.Map<ShoppingCartResponse>(shoppingCart);
        return shoppingCartResponse;
    }
}