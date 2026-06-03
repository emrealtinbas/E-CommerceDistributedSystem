using Basket.Application.Abstractions.Persistence;
using Basket.Application.Baskets.Mapping;
using Basket.Application.Baskets.Models;
using MediatR;

namespace Basket.Application.Baskets.GetBasket;

public sealed class GetBasketQueryHandler(IBasketRepository basketRepository) : IRequestHandler<GetBasketQuery, BasketDto?>
{
    public async Task<BasketDto?> Handle(GetBasketQuery request, CancellationToken cancellationToken)
    {
        var basket = await basketRepository.GetAsync(request.CustomerId, cancellationToken);

        return basket is null ? null : BasketMapper.ToDto(basket);
    }
}
