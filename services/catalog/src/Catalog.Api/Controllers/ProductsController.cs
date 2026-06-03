using Catalog.Application.Products.CreateProduct;
using Catalog.Application.Products.DeactivateProduct;
using Catalog.Application.Products.GetProductById;
using Catalog.Application.Products.ListProducts;
using Catalog.Application.Products.UpdateProduct;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Api.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var products = await mediator.Send(new ListProductsQuery(), cancellationToken);

        return Ok(products);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var product = await mediator.Send(new GetProductByIdQuery(id), cancellationToken);

        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var productId = await mediator.Send(
            new CreateProductCommand(
                request.Name,
                request.Description,
                request.Price,
                request.Currency,
                request.CategoryId),
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = productId }, new { id = productId });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(
            new UpdateProductCommand(
                id,
                request.Name,
                request.Description,
                request.Price,
                request.Currency,
                request.CategoryId,
                request.RowVersion),
            cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(Guid id, DeactivateProductRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeactivateProductCommand(id, request.RowVersion), cancellationToken);

        return NoContent();
    }
}

public sealed record CreateProductRequest(
    string Name,
    string Description,
    decimal Price,
    string Currency,
    Guid CategoryId);

public sealed record UpdateProductRequest(
    string Name,
    string Description,
    decimal Price,
    string Currency,
    Guid CategoryId,
    string RowVersion);

public sealed record DeactivateProductRequest(string RowVersion);
