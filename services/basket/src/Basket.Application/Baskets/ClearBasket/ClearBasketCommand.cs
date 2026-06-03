using MediatR;

namespace Basket.Application.Baskets.ClearBasket;

public sealed record ClearBasketCommand(string CustomerId) : IRequest;
