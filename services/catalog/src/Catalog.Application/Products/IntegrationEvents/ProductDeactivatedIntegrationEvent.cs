using Catalog.Application.Abstractions.Messaging;

namespace Catalog.Application.Products.IntegrationEvents;

public sealed record ProductDeactivatedIntegrationEvent(
    Guid ProductId,
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : IIntegrationEvent
{
    public static ProductDeactivatedIntegrationEvent Create(Guid productId)
    {
        return new ProductDeactivatedIntegrationEvent(productId, Guid.NewGuid(), DateTimeOffset.UtcNow);
    }
}
