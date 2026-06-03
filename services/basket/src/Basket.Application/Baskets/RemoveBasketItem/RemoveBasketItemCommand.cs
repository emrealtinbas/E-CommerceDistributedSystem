using Basket.Application.Baskets.Models;
using MediatR;

namespace Basket.Application.Baskets.RemoveBasketItem;

public sealed record RemoveBasketItemCommand(string CustomerId, Guid ProductId) : IRequest<BasketDto?>;
