using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Common.Exceptions;
using MediatR;

namespace Catalog.Application.Products.DeactivateProduct;

public sealed class DeactivateProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeactivateProductCommand>
{
    public async Task Handle(DeactivateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdForUpdateAsync(request.ProductId, cancellationToken)
            ?? throw new NotFoundException($"Product '{request.ProductId}' was not found.");

        product.Deactivate();

        productRepository.UseOriginalRowVersion(product, Convert.FromBase64String(request.RowVersion));
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
