namespace Basket.Domain.Entities;

public sealed class BasketItem
{
    private BasketItem()
    {
        ProductName = string.Empty;
        Currency = string.Empty;
    }

    public BasketItem(Guid productId, string productName, decimal unitPrice, string currency, int quantity)
    {
        if (productId == Guid.Empty)
        {
            throw new ArgumentException("Product id is required.", nameof(productId));
        }

        Validate(productName, unitPrice, currency, quantity);

        ProductId = productId;
        ProductName = productName.Trim();
        UnitPrice = unitPrice;
        Currency = currency.Trim().ToUpperInvariant();
        Quantity = quantity;
    }

    public Guid ProductId { get; private set; }

    public string ProductName { get; private set; }

    public decimal UnitPrice { get; private set; }

    public string Currency { get; private set; }

    public int Quantity { get; private set; }

    public decimal TotalPrice => UnitPrice * Quantity;

    public void Update(string productName, decimal unitPrice, string currency, int quantity)
    {
        Validate(productName, unitPrice, currency, quantity);

        ProductName = productName.Trim();
        UnitPrice = unitPrice;
        Currency = currency.Trim().ToUpperInvariant();
        Quantity = quantity;
    }

    private static void Validate(string productName, decimal unitPrice, string currency, int quantity)
    {
        if (string.IsNullOrWhiteSpace(productName))
        {
            throw new ArgumentException("Product name is required.", nameof(productName));
        }

        if (unitPrice < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(unitPrice), "Unit price cannot be negative.");
        }

        if (string.IsNullOrWhiteSpace(currency) || currency.Trim().Length != 3)
        {
            throw new ArgumentException("Currency must be a three-letter ISO code.", nameof(currency));
        }

        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        }
    }
}
