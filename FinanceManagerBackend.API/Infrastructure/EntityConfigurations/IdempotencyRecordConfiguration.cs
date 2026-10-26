using FinanceManagerBackend.API.Domain.Entities;
using FinanceManagerBackend.API.Infrastructure.Idempotency;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceManagerBackend.API.Infrastructure.EntityConfigurations;

/// <inheritdoc />
public class IdempotencyRecordConfiguration : IEntityTypeConfiguration<IdempotencyRecord>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<IdempotencyRecord> builder)
    {
        builder.ToTable("idempotency_keys");

        builder.HasKey(x => new { x.UserId, x.Key });

        builder
            .Property(x => x.Key)
            .HasMaxLength(KeyMaxLength);

        builder
            .Property(x => x.RequestHash)
            .HasMaxLength(64);

        builder
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.CreatedAt);
    }

    /// <summary>
    /// Max Idempotency-Key length.
    /// </summary>
    public const int KeyMaxLength = 255;
}
