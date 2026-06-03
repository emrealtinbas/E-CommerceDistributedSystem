using Basket.Application.Abstractions.Persistence;
using Basket.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Basket.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddBasketInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis")
                ?? throw new InvalidOperationException("Connection string 'Redis' is not configured.");
            options.InstanceName = "basket:";
        });

        services.AddScoped<IBasketRepository, RedisBasketRepository>();
        services.AddHealthChecks().AddCheck<RedisHealthCheck>("redis");

        return services;
    }
}
