using Basket.Application.Abstractions.Persistence;
using Basket.Application.Baskets.Mapping;
using Basket.Application.Baskets.Models;
using MediatR;

namespace Basket.Application.Baskets.PrepareCheckout;

public sealed class PrepareCheckoutQueryHandler(IBasketRepository basketRepository) : IRequestHandler<PrepareCheckoutQuery, BasketCheckoutDto?>
{
    public async Task<BasketCheckoutDto?> Handle(PrepareCheckoutQuery request, CancellationToken cancellationToken)
    {
        var basket = await basketRepository.GetAsync(request.CustomerId, cancellationToken);

        return basket is null || basket.Items.Count == 0 ? null : BasketMapper.ToCheckoutDto(basket);
    }
}
