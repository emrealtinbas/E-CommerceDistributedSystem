using Catalog.Domain.Entities;
using Catalog.Infrastructure.Persistence.SeedData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Persistence.Configurations;

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.HasKey(category => category.Id);

        builder.Property(category => category.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasData(
            new { Id = CatalogSeedData.BooksCategoryId, Name = "Books" },
            new { Id = CatalogSeedData.ElectronicsCategoryId, Name = "Electronics" });
    }
}
