namespace Catalog.Domain.Entities;

public sealed class Product
{
    private Product()
    {
        Name = string.Empty;
        Description = string.Empty;
        Currency = string.Empty;
        RowVersion = [];
    }

    public Product(Guid id, string name, string description, decimal price, string currency, Guid categoryId)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Product id cannot be empty.", nameof(id));
        }

        ValidateName(name);
        ValidatePrice(price);
        ValidateCurrency(currency);

        Id = id;
        Name = name.Trim();
        Description = description.Trim();
        Price = price;
        Currency = currency.Trim().ToUpperInvariant();
        CategoryId = categoryId;
        IsActive = true;
        CreatedAtUtc = DateTimeOffset.UtcNow;
        RowVersion = [];
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public string Description { get; private set; }

    public decimal Price { get; private set; }

    public string Currency { get; private set; }

    public Guid CategoryId { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public byte[] RowVersion { get; private set; }

    public void Rename(string name)
    {
        ValidateName(name);
        Name = name.Trim();
    }

    public void ChangePrice(decimal price, string currency)
    {
        ValidatePrice(price);
        ValidateCurrency(currency);

        Price = price;
        Currency = currency.Trim().ToUpperInvariant();
    }

    public void Deactivate() => IsActive = false;

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Product name is required.", nameof(name));
        }
    }

    private static void ValidatePrice(decimal price)
    {
        if (price < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(price), "Product price cannot be negative.");
        }
    }

    private static void ValidateCurrency(string currency)
    {
        if (string.IsNullOrWhiteSpace(currency) || currency.Trim().Length != 3)
        {
            throw new ArgumentException("Currency must be a three-letter ISO code.", nameof(currency));
        }
    }
}
