using System.Net;
using System.Net.Http.Json;
using Catalog.Application.Products.Models;
using Catalog.IntegrationTests.Infrastructure;

namespace Catalog.IntegrationTests.Api;

public sealed class ProductsApiTests(CatalogApiFactory factory) : IClassFixture<CatalogApiFactory>
{
    [DockerFact]
    public async Task Get_products_returns_seeded_catalog_items()
    {
        await factory.ApplyMigrationsAsync();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/products");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var products = await response.Content.ReadFromJsonAsync<IReadOnlyList<ProductDto>>();
        Assert.NotNull(products);
        Assert.Contains(products, product => product.Name == "Wireless Headphones");
        Assert.Contains(products, product => product.Name == "Domain-Driven Design");
    }
}
