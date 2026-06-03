using Basket.Application.Abstractions.Persistence;
using Basket.Application.Baskets.Mapping;
using Basket.Application.Baskets.Models;
using Basket.Domain.Entities;
using MediatR;

namespace Basket.Application.Baskets.AddBasketItem;

public sealed class AddBasketItemCommandHandler(IBasketRepository basketRepository) : IRequestHandler<AddBasketItemCommand, BasketDto>
{
    public async Task<BasketDto> Handle(AddBasketItemCommand request, CancellationToken cancellationToken)
    {
        var basket = await basketRepository.GetAsync(request.CustomerId, cancellationToken) ?? new CustomerBasket(request.CustomerId);

        basket.AddOrUpdateItem(request.ProductId, request.ProductName, request.UnitPrice, request.Currency, request.Quantity);
        await basketRepository.SaveAsync(basket, cancellationToken);

        return BasketMapper.ToDto(basket);
    }
}
