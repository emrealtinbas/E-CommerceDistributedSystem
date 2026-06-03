using AutoMapper;
using Catalog.Application.Abstractions.Caching;
using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Products.Models;
using MediatR;

namespace Catalog.Application.Products.ListProducts;

public sealed class ListProductsQueryHandler(
    IProductRepository productRepository,
    IProductCache productCache,
    IMapper mapper) : IRequestHandler<ListProductsQuery, IReadOnlyList<ProductDto>>
{
    public async Task<IReadOnlyList<ProductDto>> Handle(ListProductsQuery request, CancellationToken cancellationToken)
    {
        var cachedProducts = await productCache.GetProductListAsync(cancellationToken);

        if (cachedProducts is not null)
        {
            return cachedProducts;
        }

        var products = await productRepository.ListAsync(cancellationToken);
        var productDtos = mapper.Map<IReadOnlyList<ProductDto>>(products);

        await productCache.SetProductListAsync(productDtos, cancellationToken);

        return productDtos;
    }
}
