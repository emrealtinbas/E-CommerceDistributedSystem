using Catalog.Application.Products.Models;
using MediatR;

namespace Catalog.Application.Products.ListProducts;

public sealed record ListProductsQuery : IRequest<IReadOnlyList<ProductDto>>;
