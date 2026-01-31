namespace FinanceManagerBackend.API.Domain.Entities;

/// <summary>
/// Transaction entity.
/// </summary>
public class Transaction : BaseEntity
{
    /// <summary>
    /// Transaction account id.
    /// </summary>
    public Guid AccountId { get; set; }

    /// <summary>
    /// Transaction category id.
    /// </summary>
    public Guid CategoryId { get; set; }

    /// <summary>
    /// Transaction amount.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Transaction datetime.
    /// </summary>
    public DateTimeOffset DateTime { get; set; }

    /// <summary>
    /// Transaction comment.
    /// </summary>
    public string? Comment { get; set; }
}