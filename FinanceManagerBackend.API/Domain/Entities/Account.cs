namespace FinanceManagerBackend.API.Domain.Entities;

public class Account : BaseEntity
{
    public Guid UserId { get; set; }

    public string Name { get; set; }

    public decimal Balance { get; set; }

    public Guid CurrencyId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}