namespace Catalog.Infrastructure.Persistence.Idempotency;

public sealed class ProcessedMessage
{
    private ProcessedMessage()
    {
        Consumer = string.Empty;
    }

    public ProcessedMessage(Guid messageId, string consumer, DateTimeOffset processedOnUtc)
    {
        MessageId = messageId;
        Consumer = consumer;
        ProcessedOnUtc = processedOnUtc;
    }

    public Guid MessageId { get; private set; }

    public string Consumer { get; private set; }

    public DateTimeOffset ProcessedOnUtc { get; private set; }
}
