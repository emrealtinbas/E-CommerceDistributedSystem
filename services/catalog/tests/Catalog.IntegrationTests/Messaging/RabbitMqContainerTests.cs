using Catalog.IntegrationTests.Infrastructure;
using RabbitMQ.Client;
using Testcontainers.RabbitMq;

namespace Catalog.IntegrationTests.Messaging;

public sealed class RabbitMqContainerTests
{
    [DockerFact]
    public async Task Can_connect_and_declare_integration_exchange()
    {
        await using var container = new RabbitMqBuilder()
            .WithImage("rabbitmq:4.0-management-alpine")
            .WithUsername("guest")
            .WithPassword("guest")
            .Build();

        await container.StartAsync();

        var factory = new ConnectionFactory
        {
            Uri = new Uri(container.GetConnectionString())
        };

        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(
            "ecommerce.integration.test",
            ExchangeType.Topic,
            durable: true,
            autoDelete: false);
    }
}
