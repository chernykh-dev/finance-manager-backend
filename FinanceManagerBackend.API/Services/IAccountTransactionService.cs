using FinanceManagerBackend.API.Domain.Entities;

namespace FinanceManagerBackend.API.Services;

/// <summary>
/// Account transaction service.
/// </summary>
public interface IAccountTransactionService
{
    /// <summary>
    /// Update account amount by transaction.
    /// </summary>
    /// <param name="transaction"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task UpdateAccountAmountAsync(Transaction transaction,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Replace account amount by old and updated transactions.
    /// </summary>
    /// <param name="oldTransaction"></param>
    /// <param name="updatedTransaction"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task ReplaceAccountAmountAsync(Transaction oldTransaction, Transaction updatedTransaction,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Rollback account amount by transaction.
    /// </summary>
    /// <param name="transaction"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task RollbackAccountAmountAsync(Transaction transaction,
        CancellationToken cancellationToken = default);
}