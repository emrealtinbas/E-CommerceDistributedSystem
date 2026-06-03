namespace Catalog.Infrastructure.Persistence.Outbox;

public sealed class OutboxMessage
{
    private OutboxMessage()
    {
        Type = string.Empty;
        Content = string.Empty;
    }

    public OutboxMessage(Guid id, string type, string content, DateTimeOffset occurredOnUtc)
    {
        Id = id;
        Type = type;
        Content = content;
        OccurredOnUtc = occurredOnUtc;
    }

    public Guid Id { get; private set; }

    public string Type { get; private set; }

    public string Content { get; private set; }

    public DateTimeOffset OccurredOnUtc { get; private set; }

    public DateTimeOffset? ProcessedOnUtc { get; private set; }

    public string? Error { get; private set; }

    public int RetryCount { get; private set; }

    public void MarkProcessed(DateTimeOffset processedOnUtc)
    {
        ProcessedOnUtc = processedOnUtc;
        Error = null;
    }

    public void MarkFailed(string error)
    {
        Error = error;
        RetryCount++;
    }
}
