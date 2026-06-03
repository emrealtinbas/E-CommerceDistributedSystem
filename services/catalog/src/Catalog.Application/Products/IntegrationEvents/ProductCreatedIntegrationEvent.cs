using Catalog.Application.Abstractions.Messaging;

namespace Catalog.Application.Products.IntegrationEvents;

public sealed record ProductCreatedIntegrationEvent(
    Guid ProductId,
    string Name,
    decimal Price,
    string Currency,
    Guid CategoryId,
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : IIntegrationEvent
{
    public static ProductCreatedIntegrationEvent Create(
        Guid productId,
        string name,
        decimal price,
        string currency,
        Guid categoryId)
    {
        return new ProductCreatedIntegrationEvent(
            productId,
            name,
            price,
            currency,
            categoryId,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);
    }
}
