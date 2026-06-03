using Basket.Domain.Entities;

namespace Basket.UnitTests;

public sealed class CustomerBasketTests
{
    [Fact]
    public void AddOrUpdateItem_adds_new_item_and_calculates_total()
    {
        var basket = new CustomerBasket("customer-1");

        basket.AddOrUpdateItem(Guid.NewGuid(), "Wireless Headphones", 129.99m, "USD", 2);

        Assert.Single(basket.Items);
        Assert.Equal(259.98m, basket.TotalPrice);
    }

    [Fact]
    public void AddOrUpdateItem_updates_existing_item_for_same_product()
    {
        var productId = Guid.NewGuid();
        var basket = new CustomerBasket("customer-1");

        basket.AddOrUpdateItem(productId, "Wireless Headphones", 129.99m, "USD", 1);
        basket.AddOrUpdateItem(productId, "Wireless Headphones", 119.99m, "USD", 3);

        Assert.Single(basket.Items);
        Assert.Equal(359.97m, basket.TotalPrice);
    }
}
