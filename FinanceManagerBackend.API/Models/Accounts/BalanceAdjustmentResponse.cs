using FinanceManagerBackend.API.Models.Transactions;

namespace FinanceManagerBackend.API.Models.Accounts;

/// <summary>
/// Manual balance adjustment result.
/// </summary>
public class BalanceAdjustmentResponse
{
    /// <summary>
    /// Account after the adjustment.
    /// </summary>
    public AccountResponse Account { get; set; } = null!;

    /// <summary>
    /// Created adjustment transaction; null if the balance already had the requested value.
    /// </summary>
    public TransactionResponse? Transaction { get; set; }
}
