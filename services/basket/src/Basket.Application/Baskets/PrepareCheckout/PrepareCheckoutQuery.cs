using Basket.Application.Baskets.Models;
using MediatR;

namespace Basket.Application.Baskets.PrepareCheckout;

public sealed record PrepareCheckoutQuery(string CustomerId) : IRequest<BasketCheckoutDto?>;
