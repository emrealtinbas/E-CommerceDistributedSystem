using Basket.Application.Baskets.Models;
using Basket.Domain.Entities;

namespace Basket.Application.Baskets.Mapping;

internal static class BasketMapper
{
    public static BasketDto ToDto(CustomerBasket basket)
    {
        var items = basket.Items
            .Select(item => new BasketItemDto(item.ProductId, item.ProductName, item.UnitPrice, item.Currency, item.Quantity, item.TotalPrice))
            .ToList();

        return new BasketDto(basket.CustomerId, items, basket.TotalPrice);
    }

    public static BasketCheckoutDto ToCheckoutDto(CustomerBasket basket)
    {
        var dto = ToDto(basket);

        return new BasketCheckoutDto(dto.CustomerId, dto.Items, dto.TotalPrice);
    }
}
