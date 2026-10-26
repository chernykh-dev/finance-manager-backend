using FinanceManagerBackend.API.Exceptions;

namespace FinanceManagerBackend.API.Domain;

/// <summary>
/// Atomic account balance changes.
/// </summary>
public interface IAccountBalanceRepository
{
    /// <summary>
    /// Atomically add delta to account balance (balance = balance + delta) in a single statement,
    /// so concurrent changes of the same account are never lost.
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="delta">Signed amount to add.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="EntityNotFoundException"></exception>
    Task AddToBalanceAsync(Guid accountId, decimal delta, CancellationToken cancellationToken = default);
}
