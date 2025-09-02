namespace FinanceManagerBackend.API.Domain.Entities;

public class Transaction : BaseEntity
{
    public Guid AccountId { get; set; }

    public Guid CategoryId { get; set; }

    public decimal Amount { get; set; }

    public DateTimeOffset DateTime { get; set; }

    public string? Comment { get; set; }
}