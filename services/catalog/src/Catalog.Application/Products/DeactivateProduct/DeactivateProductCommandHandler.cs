using Catalog.Application.Abstractions.Caching;
using Catalog.Application.Abstractions.Messaging;
using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Common.Exceptions;
using Catalog.Application.Products.IntegrationEvents;
using MediatR;

namespace Catalog.Application.Products.DeactivateProduct;

public sealed class DeactivateProductCommandHandler(
    IProductRepository productRepository,
    IProductCache productCache,
    IOutboxWriter outboxWriter,
    IUnitOfWork unitOfWork) : IRequestHandler<DeactivateProductCommand>
{
    public async Task Handle(DeactivateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdForUpdateAsync(request.ProductId, cancellationToken)
            ?? throw new NotFoundException($"Product '{request.ProductId}' was not found.");

        product.Deactivate();

        productRepository.UseOriginalRowVersion(product, Convert.FromBase64String(request.RowVersion));
        await outboxWriter.AddAsync(ProductDeactivatedIntegrationEvent.Create(product.Id), cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await productCache.InvalidateProductAsync(request.ProductId, cancellationToken);
        await productCache.InvalidateProductListAsync(cancellationToken);
    }
}
