using Catalog.Application.Abstractions.Caching;
using Catalog.Application.Abstractions.Messaging;
using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Products.IntegrationEvents;
using Catalog.Domain.Entities;
using MediatR;

namespace Catalog.Application.Products.CreateProduct;

public sealed class CreateProductCommandHandler(
    IProductRepository productRepository,
    IProductCache productCache,
    IOutboxWriter outboxWriter,
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
        await outboxWriter.AddAsync(
            ProductCreatedIntegrationEvent.Create(
                product.Id,
                product.Name,
                product.Price,
                product.Currency,
                product.CategoryId),
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await productCache.InvalidateProductListAsync(cancellationToken);

        return product.Id;
    }
}
