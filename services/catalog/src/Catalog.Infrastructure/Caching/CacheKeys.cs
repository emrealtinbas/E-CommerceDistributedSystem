namespace Catalog.Infrastructure.Caching;

internal static class CacheKeys
{
    public const string ProductList = "catalog:products:list";

    public static string Product(Guid productId) => $"catalog:products:{productId}";
}
