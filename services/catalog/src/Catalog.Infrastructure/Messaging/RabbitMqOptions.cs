namespace Catalog.Infrastructure.Messaging;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMQ";

    public string HostName { get; init; } = "localhost";

    public int Port { get; init; } = 5672;

    public string UserName { get; init; } = "guest";

    public string Password { get; init; } = "guest";

    public string ExchangeName { get; init; } = "ecommerce.integration";

    public string DeadLetterExchangeName { get; init; } = "ecommerce.integration.dlx";

    public int OutboxBatchSize { get; init; } = 20;

    public int OutboxPollingIntervalSeconds { get; init; } = 5;

    public int MaxRetryCount { get; init; } = 5;
}
