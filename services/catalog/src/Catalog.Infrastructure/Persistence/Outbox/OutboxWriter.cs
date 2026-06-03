using System.Text.Json;
using Catalog.Application.Abstractions.Messaging;

namespace Catalog.Infrastructure.Persistence.Outbox;

internal sealed class OutboxWriter(CatalogDbContext dbContext) : IOutboxWriter
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task AddAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        var message = new OutboxMessage(
            integrationEvent.EventId,
            integrationEvent.GetType().FullName ?? integrationEvent.GetType().Name,
            JsonSerializer.Serialize(integrationEvent, integrationEvent.GetType(), SerializerOptions),
            integrationEvent.OccurredOnUtc);

        await dbContext.OutboxMessages.AddAsync(message, cancellationToken);
    }
}
