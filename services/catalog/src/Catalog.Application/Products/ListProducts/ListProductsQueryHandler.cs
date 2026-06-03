using AutoMapper;
using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Products.Models;
using MediatR;

namespace Catalog.Application.Products.ListProducts;

public sealed class ListProductsQueryHandler(
    IProductRepository productRepository,
    IMapper mapper) : IRequestHandler<ListProductsQuery, IReadOnlyList<ProductDto>>
{
    public async Task<IReadOnlyList<ProductDto>> Handle(ListProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await productRepository.ListAsync(cancellationToken);

        return mapper.Map<IReadOnlyList<ProductDto>>(products);
    }
}
