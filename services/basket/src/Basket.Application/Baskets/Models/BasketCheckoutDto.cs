namespace Basket.Application.Baskets.Models;

public sealed record BasketCheckoutDto(string CustomerId, IReadOnlyCollection<BasketItemDto> Items, decimal TotalPrice);
