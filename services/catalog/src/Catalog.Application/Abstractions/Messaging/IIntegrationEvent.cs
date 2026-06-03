namespace Catalog.Application.Abstractions.Messaging;

public interface IIntegrationEvent
{
    Guid EventId { get; }

    DateTimeOffset OccurredOnUtc { get; }
}
