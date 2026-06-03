using Catalog.Application.Products.Models;
using MediatR;

namespace Catalog.Application.Products.GetProductById;

public sealed record GetProductByIdQuery(Guid ProductId) : IRequest<ProductDto?>;
