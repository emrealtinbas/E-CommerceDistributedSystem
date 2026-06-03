using Catalog.Application.Abstractions.Caching;
using Catalog.Application.Abstractions.Messaging;
using Catalog.Application.Abstractions.Persistence;
using Catalog.Infrastructure.Caching;
using Catalog.Infrastructure.Messaging;
using Catalog.Infrastructure.Persistence;
using Catalog.Infrastructure.Persistence.Outbox;
using Catalog.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCatalogInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("CatalogDb")
            ?? throw new InvalidOperationException("Connection string 'CatalogDb' is not configured.");

        services.AddDbContext<CatalogDbContext>(options => options.UseSqlServer(connectionString));
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis")
                ?? throw new InvalidOperationException("Connection string 'Redis' is not configured.");
            options.InstanceName = "catalog:";
        });

        var rabbitMqSection = configuration.GetSection(RabbitMqOptions.SectionName);
        services.AddSingleton(new RabbitMqOptions
        {
            HostName = rabbitMqSection["HostName"] ?? "localhost",
            Port = int.TryParse(rabbitMqSection["Port"], out var port) ? port : 5672,
            UserName = rabbitMqSection["UserName"] ?? "guest",
            Password = rabbitMqSection["Password"] ?? "guest",
            ExchangeName = rabbitMqSection["ExchangeName"] ?? "ecommerce.integration",
            DeadLetterExchangeName = rabbitMqSection["DeadLetterExchangeName"] ?? "ecommerce.integration.dlx",
            OutboxBatchSize = int.TryParse(rabbitMqSection["OutboxBatchSize"], out var batchSize) ? batchSize : 20,
            OutboxPollingIntervalSeconds = int.TryParse(rabbitMqSection["OutboxPollingIntervalSeconds"], out var pollingIntervalSeconds)
                ? pollingIntervalSeconds
                : 5,
            MaxRetryCount = int.TryParse(rabbitMqSection["MaxRetryCount"], out var maxRetryCount) ? maxRetryCount : 5,
            ClaimTimeoutSeconds = int.TryParse(rabbitMqSection["ClaimTimeoutSeconds"], out var claimTimeoutSeconds) ? claimTimeoutSeconds : 60
        });
        services.AddScoped<IProductCache, RedisProductCache>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOutboxWriter, OutboxWriter>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddHostedService<OutboxPublisherService>();
        services.AddHealthChecks()
            .AddDbContextCheck<CatalogDbContext>("catalog-db")
            .AddCheck<RabbitMqHealthCheck>("rabbitmq");

        return services;
    }
}
