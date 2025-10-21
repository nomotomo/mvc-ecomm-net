using AutoMapper;
using Basket.Application.Responses;
using Basket.Core.Entities;

namespace Basket.Application.Mappers;

public class BasketMappingProfiles : Profile
{
    public BasketMappingProfiles()
    {
        CreateMap<ShoppingCart, ShoppingCartResponse>().ReverseMap();
        CreateMap<ShoppingCartItem, ShoppingCartItemResponse>().ReverseMap();
    }
}