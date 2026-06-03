using Catalog.Application.Products.Models;

namespace Catalog.Application.Abstractions.Caching;

public interface IProductCache
{
    Task<IReadOnlyList<ProductDto>?> GetProductListAsync(CancellationToken cancellationToken);

    Task SetProductListAsync(IReadOnlyList<ProductDto> products, CancellationToken cancellationToken);

    Task<ProductDto?> GetProductAsync(Guid productId, CancellationToken cancellationToken);

    Task SetProductAsync(ProductDto product, CancellationToken cancellationToken);

    Task InvalidateProductAsync(Guid productId, CancellationToken cancellationToken);

    Task InvalidateProductListAsync(CancellationToken cancellationToken);
}
