using System.ComponentModel.DataAnnotations;

namespace FinanceManagerBackend.API.Models.Accounts;

/// <summary>
/// Manual balance adjustment request: set the account balance to the given value.
/// </summary>
public class BalanceAdjustmentRequest
{
    /// <summary>
    /// Target account balance. The server records the difference with the current balance as a transaction.
    /// </summary>
    [Required]
    public decimal Balance { get; set; }

    /// <summary>
    /// Adjustment transaction datetime (default is now).
    /// </summary>
    public DateTimeOffset? DateTime { get; set; }

    /// <summary>
    /// Adjustment transaction comment.
    /// </summary>
    public string? Comment { get; set; }
}
