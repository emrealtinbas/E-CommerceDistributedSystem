using AutoMapper;
using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Products.Models;
using MediatR;

namespace Catalog.Application.Products.GetProductById;

public sealed class GetProductByIdQueryHandler(
    IProductRepository productRepository,
    IMapper mapper) : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);

        return product is null ? null : mapper.Map<ProductDto>(product);
    }
}
