using Catalog.Domain.Entities;

namespace Catalog.Application.Abstractions.Persistence;

public interface IProductRepository
{
    Task AddAsync(Product product, CancellationToken cancellationToken);

    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Product?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken);

    void UseOriginalRowVersion(Product product, byte[] rowVersion);

    Task<IReadOnlyList<Product>> ListAsync(CancellationToken cancellationToken);
}
