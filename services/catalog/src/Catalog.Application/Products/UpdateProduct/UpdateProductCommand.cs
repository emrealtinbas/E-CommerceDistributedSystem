using MediatR;

namespace Catalog.Application.Products.UpdateProduct;

public sealed record UpdateProductCommand(
    Guid ProductId,
    string Name,
    string Description,
    decimal Price,
    string Currency,
    Guid CategoryId,
    string RowVersion) : IRequest;
