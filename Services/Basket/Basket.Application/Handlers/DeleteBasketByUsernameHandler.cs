using Basket.Application.Commands;
using Basket.Core.Repositories;
using MediatR;

namespace Basket.Application.Handlers;

public class DeleteBasketByUsernameHandler : IRequestHandler<DeleteBasketByUserNameCommand>
{
    private readonly IBasketRepository _basketRepository;

    public DeleteBasketByUsernameHandler(IBasketRepository basketRepository)
    {
        
    }
    public async Task Handle(DeleteBasketByUserNameCommand request, CancellationToken cancellationToken)
    {
        await _basketRepository.DeleteBasket(request.UserName);
    }
}
