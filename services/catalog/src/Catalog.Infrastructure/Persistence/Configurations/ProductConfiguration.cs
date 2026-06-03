using Catalog.Domain.Entities;
using Catalog.Infrastructure.Persistence.SeedData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Persistence.Configurations;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(product => product.Id);

        builder.Property(product => product.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(product => product.Description)
            .HasMaxLength(2_000)
            .IsRequired();

        builder.Property(product => product.Price)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(product => product.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(product => product.RowVersion)
            .IsRowVersion();

        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(product => product.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(product => product.Name);

        builder.HasData(
            new
            {
                Id = CatalogSeedData.HeadphonesProductId,
                Name = "Wireless Headphones",
                Description = "Noise-cancelling wireless headphones for daily use.",
                Price = 129.99m,
                Currency = "USD",
                CategoryId = CatalogSeedData.ElectronicsCategoryId,
                IsActive = true,
                CreatedAtUtc = CatalogSeedData.CreatedAtUtc
            },
            new
            {
                Id = CatalogSeedData.DddBookProductId,
                Name = "Domain-Driven Design",
                Description = "A strategic design book for complex software.",
                Price = 49.99m,
                Currency = "USD",
                CategoryId = CatalogSeedData.BooksCategoryId,
                IsActive = true,
                CreatedAtUtc = CatalogSeedData.CreatedAtUtc
            });
    }
}
