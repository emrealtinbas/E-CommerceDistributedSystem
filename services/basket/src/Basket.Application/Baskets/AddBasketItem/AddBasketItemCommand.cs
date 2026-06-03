using Basket.Application.Baskets.Models;
using MediatR;

namespace Basket.Application.Baskets.AddBasketItem;

public sealed record AddBasketItemCommand(
    string CustomerId,
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    string Currency,
    int Quantity) : IRequest<BasketDto>;
