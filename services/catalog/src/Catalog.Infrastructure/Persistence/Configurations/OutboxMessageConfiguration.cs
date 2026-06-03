using Catalog.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Persistence.Configurations;

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");
        builder.HasKey(message => message.Id);

        builder.Property(message => message.Type)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(message => message.Content)
            .IsRequired();

        builder.Property(message => message.Error)
            .HasMaxLength(2_000);

        builder.HasIndex(message => new { message.ProcessedOnUtc, message.DeadLetteredOnUtc, message.OccurredOnUtc });
        builder.HasIndex(message => message.LockId);
    }
}
