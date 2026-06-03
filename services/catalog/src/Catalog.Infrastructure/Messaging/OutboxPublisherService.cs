using System.Text;
using Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace Catalog.Infrastructure.Messaging;

internal sealed class OutboxPublisherService(
    IServiceScopeFactory scopeFactory,
    RabbitMqOptions options,
    ILogger<OutboxPublisherService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var pollingInterval = TimeSpan.FromSeconds(options.OutboxPollingIntervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PublishPendingMessagesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Outbox publisher cycle failed.");
            }

            await Task.Delay(pollingInterval, stoppingToken);
        }
    }

    private async Task PublishPendingMessagesAsync(CancellationToken cancellationToken)
    {
        await using var connection = await CreateConnectionAsync(cancellationToken);
        await using var channel = await connection.CreateChannelAsync(
            new CreateChannelOptions(
                publisherConfirmationsEnabled: true,
                publisherConfirmationTrackingEnabled: true),
            cancellationToken);

        await DeclareTopologyAsync(channel, cancellationToken);

        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

        var messages = await dbContext.OutboxMessages
            .Where(message => message.ProcessedOnUtc == null && message.DeadLetteredOnUtc == null)
            .OrderBy(message => message.OccurredOnUtc)
            .Take(options.OutboxBatchSize)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                if (message.RetryCount >= options.MaxRetryCount)
                {
                    await PublishDeadLetterAsync(channel, message.Id, message.Type, message.Content, cancellationToken);
                    message.MarkDeadLettered(DateTimeOffset.UtcNow, "Maximum retry count exceeded.");
                    await dbContext.SaveChangesAsync(cancellationToken);
                    continue;
                }

                await PublishIntegrationEventAsync(channel, message.Id, message.Type, message.Content, cancellationToken);
                message.MarkProcessed(DateTimeOffset.UtcNow);
            }
            catch (PublishException exception)
            {
                message.MarkFailed(exception.Message);
                logger.LogWarning(exception, "RabbitMQ rejected outbox message {OutboxMessageId}.", message.Id);
            }
            catch (Exception exception)
            {
                message.MarkFailed(exception.Message);
                logger.LogWarning(exception, "Failed to publish outbox message {OutboxMessageId}.", message.Id);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private Task<IConnection> CreateConnectionAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = options.HostName,
            Port = options.Port,
            UserName = options.UserName,
            Password = options.Password,
            AutomaticRecoveryEnabled = true,
            TopologyRecoveryEnabled = true
        };

        return factory.CreateConnectionAsync("catalog-outbox-publisher", cancellationToken);
    }

    private async Task DeclareTopologyAsync(IChannel channel, CancellationToken cancellationToken)
    {
        await channel.ExchangeDeclareAsync(
            options.ExchangeName,
            ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(
            options.DeadLetterExchangeName,
            ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);
    }

    private async Task PublishIntegrationEventAsync(
        IChannel channel,
        Guid messageId,
        string eventType,
        string content,
        CancellationToken cancellationToken)
    {
        var properties = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent,
            MessageId = messageId.ToString(),
            Type = eventType,
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        };

        var body = Encoding.UTF8.GetBytes(content);

        await channel.BasicPublishAsync(
            options.ExchangeName,
            GetRoutingKey(eventType),
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);
    }

    private async Task PublishDeadLetterAsync(
        IChannel channel,
        Guid messageId,
        string eventType,
        string content,
        CancellationToken cancellationToken)
    {
        var properties = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent,
            MessageId = messageId.ToString(),
            Type = eventType,
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        };

        await channel.BasicPublishAsync(
            options.DeadLetterExchangeName,
            $"dead-letter.{GetRoutingKey(eventType)}",
            mandatory: false,
            basicProperties: properties,
            body: Encoding.UTF8.GetBytes(content),
            cancellationToken: cancellationToken);
    }

    private static string GetRoutingKey(string eventType)
    {
        var eventName = eventType.Split('.').Last();

        return $"catalog.{eventName}";
    }
}
