namespace Basket.Domain.Entities;

public sealed class CustomerBasket
{
    private readonly List<BasketItem> _items = [];

    public CustomerBasket(string customerId)
    {
        if (string.IsNullOrWhiteSpace(customerId))
        {
            throw new ArgumentException("Customer id is required.", nameof(customerId));
        }

        CustomerId = customerId.Trim();
    }

    public string CustomerId { get; private set; }

    public IReadOnlyCollection<BasketItem> Items => _items.AsReadOnly();

    public decimal TotalPrice => _items.Sum(item => item.TotalPrice);

    public void AddOrUpdateItem(Guid productId, string productName, decimal unitPrice, string currency, int quantity)
    {
        var item = new BasketItem(productId, productName, unitPrice, currency, quantity);
        var existing = _items.FirstOrDefault(current => current.ProductId == productId);

        if (existing is null)
        {
            _items.Add(item);
            return;
        }

        existing.Update(productName, unitPrice, currency, quantity);
    }

    public void RemoveItem(Guid productId)
    {
        _items.RemoveAll(item => item.ProductId == productId);
    }
}
