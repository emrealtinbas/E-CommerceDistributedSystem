using Catalog.Application.Abstractions.Messaging;

namespace Catalog.Application.Products.IntegrationEvents;

public sealed record ProductUpdatedIntegrationEvent(
    Guid ProductId,
    string Name,
    decimal Price,
    string Currency,
    Guid CategoryId,
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : IIntegrationEvent
{
    public static ProductUpdatedIntegrationEvent Create(
        Guid productId,
        string name,
        decimal price,
        string currency,
        Guid categoryId)
    {
        return new ProductUpdatedIntegrationEvent(
            productId,
            name,
            price,
            currency,
            categoryId,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);
    }
}
