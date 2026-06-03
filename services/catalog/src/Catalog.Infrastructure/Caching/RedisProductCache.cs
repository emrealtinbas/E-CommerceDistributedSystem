using System.Text.Json;
using Catalog.Application.Abstractions.Caching;
using Catalog.Application.Products.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Catalog.Infrastructure.Caching;

internal sealed class RedisProductCache(
    IDistributedCache cache,
    ILogger<RedisProductCache> logger) : IProductCache
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private static readonly DistributedCacheEntryOptions ProductOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
    };

    private static readonly DistributedCacheEntryOptions ProductListOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
    };

    public async Task<IReadOnlyList<ProductDto>?> GetProductListAsync(CancellationToken cancellationToken)
    {
        return await GetAsync<IReadOnlyList<ProductDto>>(CacheKeys.ProductList, cancellationToken);
    }

    public async Task SetProductListAsync(IReadOnlyList<ProductDto> products, CancellationToken cancellationToken)
    {
        await SetAsync(CacheKeys.ProductList, products, ProductListOptions, cancellationToken);
    }

    public async Task<ProductDto?> GetProductAsync(Guid productId, CancellationToken cancellationToken)
    {
        return await GetAsync<ProductDto>(CacheKeys.Product(productId), cancellationToken);
    }

    public async Task SetProductAsync(ProductDto product, CancellationToken cancellationToken)
    {
        await SetAsync(CacheKeys.Product(product.Id), product, ProductOptions, cancellationToken);
    }

    public async Task InvalidateProductAsync(Guid productId, CancellationToken cancellationToken)
    {
        await RemoveAsync(CacheKeys.Product(productId), cancellationToken);
    }

    public async Task InvalidateProductListAsync(CancellationToken cancellationToken)
    {
        await RemoveAsync(CacheKeys.ProductList, cancellationToken);
    }

    private async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken)
    {
        try
        {
            var json = await cache.GetStringAsync(key, cancellationToken);

            return json is null ? default : JsonSerializer.Deserialize<T>(json, SerializerOptions);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Redis cache read failed for key {CacheKey}.", key);

            return default;
        }
    }

    private async Task SetAsync<T>(
        string key,
        T value,
        DistributedCacheEntryOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var json = JsonSerializer.Serialize(value, SerializerOptions);

            await cache.SetStringAsync(key, json, options, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Redis cache write failed for key {CacheKey}.", key);
        }
    }

    private async Task RemoveAsync(string key, CancellationToken cancellationToken)
    {
        try
        {
            await cache.RemoveAsync(key, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Redis cache invalidation failed for key {CacheKey}.", key);
        }
    }
}
