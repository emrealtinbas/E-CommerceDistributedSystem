using MediatR;

namespace Catalog.Application.Products.CreateProduct;

public sealed record CreateProductCommand(
    string Name,
    string Description,
    decimal Price,
    string Currency,
    Guid CategoryId) : IRequest<Guid>;
