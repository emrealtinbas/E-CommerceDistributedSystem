using Basket.Application.Abstractions.Persistence;
using MediatR;

namespace Basket.Application.Baskets.ClearBasket;

public sealed class ClearBasketCommandHandler(IBasketRepository basketRepository) : IRequestHandler<ClearBasketCommand>
{
    public async Task Handle(ClearBasketCommand request, CancellationToken cancellationToken)
    {
        await basketRepository.DeleteAsync(request.CustomerId, cancellationToken);
    }
}
