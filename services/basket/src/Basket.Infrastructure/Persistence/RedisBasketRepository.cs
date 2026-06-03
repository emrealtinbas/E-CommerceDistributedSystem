using System.Text.Json;
using Basket.Application.Abstractions.Persistence;
using Basket.Domain.Entities;
using Microsoft.Extensions.Caching.Distributed;

namespace Basket.Infrastructure.Persistence;

internal sealed class RedisBasketRepository(IDistributedCache cache) : IBasketRepository
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        IncludeFields = false
    };

    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        SlidingExpiration = TimeSpan.FromHours(2),
        AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7)
    };

    public async Task<CustomerBasket?> GetAsync(string customerId, CancellationToken cancellationToken)
    {
        var json = await cache.GetStringAsync(GetKey(customerId), cancellationToken);

        if (json is null)
        {
            return null;
        }

        var model = JsonSerializer.Deserialize<BasketModel>(json, SerializerOptions);

        if (model is null)
        {
            return null;
        }

        var basket = new CustomerBasket(model.CustomerId);

        foreach (var item in model.Items)
        {
            basket.AddOrUpdateItem(item.ProductId, item.ProductName, item.UnitPrice, item.Currency, item.Quantity);
        }

        return basket;
    }

    public async Task SaveAsync(CustomerBasket basket, CancellationToken cancellationToken)
    {
        var model = new BasketModel(
            basket.CustomerId,
            basket.Items.Select(item => new BasketItemModel(
                item.ProductId,
                item.ProductName,
                item.UnitPrice,
                item.Currency,
                item.Quantity)).ToList());

        var json = JsonSerializer.Serialize(model, SerializerOptions);

        await cache.SetStringAsync(GetKey(basket.CustomerId), json, CacheOptions, cancellationToken);
    }

    public async Task DeleteAsync(string customerId, CancellationToken cancellationToken)
    {
        await cache.RemoveAsync(GetKey(customerId), cancellationToken);
    }

    private static string GetKey(string customerId) => $"basket:{customerId}";

    private sealed record BasketModel(string CustomerId, IReadOnlyCollection<BasketItemModel> Items);

    private sealed record BasketItemModel(Guid ProductId, string ProductName, decimal UnitPrice, string Currency, int Quantity);
}
