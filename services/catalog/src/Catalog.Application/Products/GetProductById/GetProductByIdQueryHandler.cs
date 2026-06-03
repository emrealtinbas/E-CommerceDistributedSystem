using AutoMapper;
using Catalog.Application.Abstractions.Caching;
using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Products.Models;
using MediatR;

namespace Catalog.Application.Products.GetProductById;

public sealed class GetProductByIdQueryHandler(
    IProductRepository productRepository,
    IProductCache productCache,
    IMapper mapper) : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var cachedProduct = await productCache.GetProductAsync(request.ProductId, cancellationToken);

        if (cachedProduct is not null)
        {
            return cachedProduct;
        }

        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
        {
            return null;
        }

        var productDto = mapper.Map<ProductDto>(product);
        await productCache.SetProductAsync(productDto, cancellationToken);

        return productDto;
    }
}
