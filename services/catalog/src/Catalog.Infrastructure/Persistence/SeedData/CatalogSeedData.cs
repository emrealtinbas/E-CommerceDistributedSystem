namespace Catalog.Infrastructure.Persistence.SeedData;

internal static class CatalogSeedData
{
    public static readonly Guid BooksCategoryId = Guid.Parse("2f77fcb1-7f98-49f0-9546-6dd56a8ebf19");
    public static readonly Guid ElectronicsCategoryId = Guid.Parse("2f677466-b64d-4d92-a906-4337c8d71e84");
    public static readonly Guid HeadphonesProductId = Guid.Parse("91ce07a2-b2fe-4de6-a8ef-498625bfedb5");
    public static readonly Guid DddBookProductId = Guid.Parse("d745a731-6cb4-40fd-a38d-c8ea62e24d4c");
    public static readonly DateTimeOffset CreatedAtUtc = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
}
