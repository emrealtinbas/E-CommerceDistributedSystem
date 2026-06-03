using Catalog.Application.Abstractions.Caching;
using Catalog.Application.Abstractions.Persistence;
using Catalog.Domain.Entities;
using MediatR;

namespace Catalog.Application.Products.CreateProduct;

public sealed class CreateProductCommandHandler(
    IProductRepository productRepository,
    IProductCache productCache,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateProductCommand, Guid>
{
    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product(
            Guid.NewGuid(),
            request.Name,
            request.Description,
            request.Price,
            request.Currency,
            request.CategoryId);

        await productRepository.AddAsync(product, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await productCache.InvalidateProductListAsync(cancellationToken);

        return product.Id;
    }
}
