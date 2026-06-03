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

    public DateTimeOffset? DeadLetteredOnUtc { get; private set; }

    public Guid? LockId { get; private set; }

    public DateTimeOffset? LockedOnUtc { get; private set; }

    public string? Error { get; private set; }

    public int RetryCount { get; private set; }

    public void MarkProcessed(DateTimeOffset processedOnUtc)
    {
        ProcessedOnUtc = processedOnUtc;
        LockId = null;
        LockedOnUtc = null;
        Error = null;
    }

    public void MarkFailed(string error)
    {
        Error = error;
        RetryCount++;
        LockId = null;
        LockedOnUtc = null;
    }

    public void MarkDeadLettered(DateTimeOffset deadLetteredOnUtc, string error)
    {
        DeadLetteredOnUtc = deadLetteredOnUtc;
        LockId = null;
        LockedOnUtc = null;
        Error = error;
    }
}
