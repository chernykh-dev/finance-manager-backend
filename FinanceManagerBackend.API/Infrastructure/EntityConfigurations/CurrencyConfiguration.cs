using FinanceManagerBackend.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceManagerBackend.API.Infrastructure.EntityConfigurations;

/// <inheritdoc />
public class CurrencyConfiguration : IEntityTypeConfiguration<Currency>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Currency> builder)
    {
        builder
            .Property(x => x.Code)
            .HasMaxLength(3)
            .IsFixedLength();

        builder
            .Property(x => x.Symbol)
            .HasMaxLength(3);

        builder
            .HasIndex(x => x.Code)
            .IsUnique();
    }
}