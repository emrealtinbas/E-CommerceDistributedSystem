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
            ExchangeName = rabbitMqSection["ExchangeName"] ?? "ecommerce.integration"
        });
        services.AddScoped<IProductCache, RedisProductCache>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOutboxWriter, OutboxWriter>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddHealthChecks().AddDbContextCheck<CatalogDbContext>("catalog-db");

        return services;
    }
}
