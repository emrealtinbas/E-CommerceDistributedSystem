using Catalog.Application.Abstractions.Caching;
using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Common.Exceptions;
using MediatR;

namespace Catalog.Application.Products.UpdateProduct;

public sealed class UpdateProductCommandHandler(
    IProductRepository productRepository,
    IProductCache productCache,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateProductCommand>
{
    public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdForUpdateAsync(request.ProductId, cancellationToken)
            ?? throw new NotFoundException($"Product '{request.ProductId}' was not found.");

        product.UpdateDetails(
            request.Name,
            request.Description,
            request.Price,
            request.Currency,
            request.CategoryId);

        productRepository.UseOriginalRowVersion(product, Convert.FromBase64String(request.RowVersion));
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await productCache.InvalidateProductAsync(request.ProductId, cancellationToken);
        await productCache.InvalidateProductListAsync(cancellationToken);
    }
}
