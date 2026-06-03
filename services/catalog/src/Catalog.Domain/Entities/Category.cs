namespace Catalog.Domain.Entities;

public sealed class Category
{
    private Category()
    {
        Name = string.Empty;
    }

    public Category(Guid id, string name)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Category id cannot be empty.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Category name is required.", nameof(name));
        }

        Id = id;
        Name = name.Trim();
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; }
}
