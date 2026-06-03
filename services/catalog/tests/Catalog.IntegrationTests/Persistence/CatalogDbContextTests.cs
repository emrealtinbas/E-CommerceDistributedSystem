using Catalog.Domain.Entities;
using Catalog.IntegrationTests.Infrastructure;
using Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Catalog.IntegrationTests.Persistence;

public sealed class CatalogDbContextTests(CatalogDatabaseFixture fixture) : IClassFixture<CatalogDatabaseFixture>
{
    [DockerFact]
    public async Task Can_apply_migrations_and_persist_product()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.MigrateAsync();

        var category = new Category(Guid.NewGuid(), "Books");
        var product = new Product(
            Guid.NewGuid(),
            "Domain-Driven Design",
            "A strategic design book for complex software.",
            49.99m,
            "USD",
            category.Id);

        dbContext.Categories.Add(category);
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var storedProduct = await dbContext.Products
            .AsNoTracking()
            .SingleAsync(item => item.Id == product.Id);

        Assert.Equal("Domain-Driven Design", storedProduct.Name);
        Assert.Equal(49.99m, storedProduct.Price);
        Assert.Equal("USD", storedProduct.Currency);
    }

    private CatalogDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseSqlServer(fixture.ConnectionString)
            .Options;

        return new CatalogDbContext(options);
    }
}
