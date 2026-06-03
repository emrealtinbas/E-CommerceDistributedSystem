using MediatR;

namespace Catalog.Application.Products.DeactivateProduct;

public sealed record DeactivateProductCommand(Guid ProductId, string RowVersion) : IRequest;
