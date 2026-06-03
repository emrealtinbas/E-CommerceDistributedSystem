using Basket.Application.Baskets.Models;
using MediatR;

namespace Basket.Application.Baskets.GetBasket;

public sealed record GetBasketQuery(string CustomerId) : IRequest<BasketDto?>;
