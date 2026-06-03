using System.Net;
using System.Net.Http.Json;
using Basket.Application.Baskets.Models;
using Basket.IntegrationTests.Infrastructure;

namespace Basket.IntegrationTests.Api;

public sealed class BasketsApiTests(BasketApiFactory factory) : IClassFixture<BasketApiFactory>
{
    [DockerFact]
    public async Task Can_add_get_remove_and_prepare_checkout_for_basket()
    {
        using var client = factory.CreateClient();
        var customerId = $"customer-{Guid.NewGuid():N}";
        var productId = Guid.NewGuid();

        var addResponse = await client.PostAsJsonAsync(
            $"/api/baskets/{customerId}/items",
            new AddBasketItemRequest(productId, "Wireless Headphones", 129.99m, "USD", 2));

        Assert.Equal(HttpStatusCode.OK, addResponse.StatusCode);
        var addedBasket = await addResponse.Content.ReadFromJsonAsync<BasketDto>();
        Assert.NotNull(addedBasket);
        Assert.Equal(259.98m, addedBasket.TotalPrice);

        var getResponse = await client.GetAsync($"/api/baskets/{customerId}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var storedBasket = await getResponse.Content.ReadFromJsonAsync<BasketDto>();
        Assert.NotNull(storedBasket);
        Assert.Single(storedBasket.Items);

        var checkoutResponse = await client.GetAsync($"/api/baskets/{customerId}/checkout");

        Assert.Equal(HttpStatusCode.OK, checkoutResponse.StatusCode);
        var checkout = await checkoutResponse.Content.ReadFromJsonAsync<BasketCheckoutDto>();
        Assert.NotNull(checkout);
        Assert.Equal(customerId, checkout.CustomerId);

        var removeResponse = await client.DeleteAsync($"/api/baskets/{customerId}/items/{productId}");

        Assert.Equal(HttpStatusCode.OK, removeResponse.StatusCode);
        var basketAfterRemove = await removeResponse.Content.ReadFromJsonAsync<BasketDto>();
        Assert.NotNull(basketAfterRemove);
        Assert.Empty(basketAfterRemove.Items);
    }

    private sealed record AddBasketItemRequest(Guid ProductId, string ProductName, decimal UnitPrice, string Currency, int Quantity);
}
