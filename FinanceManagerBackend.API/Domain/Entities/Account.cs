namespace FinanceManagerBackend.API.Domain.Entities;

/// <summary>
/// Account entity.
/// </summary>
public class Account : BaseEntity
{
    /// <summary>
    /// User id.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Account name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Account balance.
    /// </summary>
    public decimal Balance { get; set; }

    /// <summary>
    /// Account currency id.
    /// </summary>
    public Guid CurrencyId { get; set; }
}