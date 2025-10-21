using Basket.Application.Responses;
using MediatR;

namespace Basket.Application.Queries;

public class GetBasketByUsernameQuery : IRequest<ShoppingCartResponse>
{
    public string Username { get; set; }

    public GetBasketByUsernameQuery(string username)
    {
        Username = username;
    }
}