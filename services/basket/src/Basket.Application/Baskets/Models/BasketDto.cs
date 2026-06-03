namespace Basket.Application.Baskets.Models;

public sealed record BasketDto(string CustomerId, IReadOnlyCollection<BasketItemDto> Items, decimal TotalPrice);

public sealed record BasketItemDto(Guid ProductId, string ProductName, decimal UnitPrice, string Currency, int Quantity, decimal TotalPrice);
