using Basket.Application.Abstractions.Persistence;
using Basket.Application.Baskets.Mapping;
using Basket.Application.Baskets.Models;
using MediatR;

namespace Basket.Application.Baskets.RemoveBasketItem;

public sealed class RemoveBasketItemCommandHandler(IBasketRepository basketRepository) : IRequestHandler<RemoveBasketItemCommand, BasketDto?>
{
    public async Task<BasketDto?> Handle(RemoveBasketItemCommand request, CancellationToken cancellationToken)
    {
        var basket = await basketRepository.GetAsync(request.CustomerId, cancellationToken);

        if (basket is null)
        {
            return null;
        }

        basket.RemoveItem(request.ProductId);
        await basketRepository.SaveAsync(basket, cancellationToken);

        return BasketMapper.ToDto(basket);
    }
}
