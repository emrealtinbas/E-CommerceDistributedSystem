using Basket.Application.Baskets.AddBasketItem;
using Basket.Application.Baskets.ClearBasket;
using Basket.Application.Baskets.GetBasket;
using Basket.Application.Baskets.PrepareCheckout;
using Basket.Application.Baskets.RemoveBasketItem;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Basket.Api.Controllers;

[ApiController]
[Route("api/baskets")]
public sealed class BasketsController(IMediator mediator) : ControllerBase
{
    [HttpGet("{customerId}")]
    public async Task<IActionResult> Get(string customerId, CancellationToken cancellationToken)
    {
        var basket = await mediator.Send(new GetBasketQuery(customerId), cancellationToken);

        return basket is null ? NotFound() : Ok(basket);
    }

    [HttpPost("{customerId}/items")]
    public async Task<IActionResult> AddOrUpdateItem(string customerId, AddBasketItemRequest request, CancellationToken cancellationToken)
    {
        var basket = await mediator.Send(
            new AddBasketItemCommand(
                customerId,
                request.ProductId,
                request.ProductName,
                request.UnitPrice,
                request.Currency,
                request.Quantity),
            cancellationToken);

        return Ok(basket);
    }

    [HttpDelete("{customerId}/items/{productId:guid}")]
    public async Task<IActionResult> RemoveItem(string customerId, Guid productId, CancellationToken cancellationToken)
    {
        var basket = await mediator.Send(new RemoveBasketItemCommand(customerId, productId), cancellationToken);

        return basket is null ? NotFound() : Ok(basket);
    }

    [HttpDelete("{customerId}")]
    public async Task<IActionResult> Clear(string customerId, CancellationToken cancellationToken)
    {
        await mediator.Send(new ClearBasketCommand(customerId), cancellationToken);

        return NoContent();
    }

    [HttpGet("{customerId}/checkout")]
    public async Task<IActionResult> PrepareCheckout(string customerId, CancellationToken cancellationToken)
    {
        var checkout = await mediator.Send(new PrepareCheckoutQuery(customerId), cancellationToken);

        return checkout is null ? NotFound() : Ok(checkout);
    }
}

public sealed record AddBasketItemRequest(Guid ProductId, string ProductName, decimal UnitPrice, string Currency, int Quantity);
