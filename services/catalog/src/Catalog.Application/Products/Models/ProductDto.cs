namespace Catalog.Application.Products.Models;

public sealed record ProductDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    string Currency,
    Guid CategoryId,
    bool IsActive,
    string RowVersion);
