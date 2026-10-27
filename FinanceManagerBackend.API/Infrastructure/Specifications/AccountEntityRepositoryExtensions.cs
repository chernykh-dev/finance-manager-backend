using FinanceManagerBackend.API.Domain;
using FinanceManagerBackend.API.Domain.Entities;
using FinanceManagerBackend.API.Exceptions;

namespace FinanceManagerBackend.API.Infrastructure.Specifications;

/// <summary>
/// Extensions methods for IEntityRepository&lt;Account&gt;.
/// </summary>
public static class AccountEntityRepositoryExtensions
{
    /// <summary>
    /// Atomically add delta to account balance (balance = balance + delta) in a single statement,
    /// so concurrent changes of the same account are never lost.
    /// </summary>
    /// <param name="accountRepository"></param>
    /// <param name="accountId"></param>
    /// <param name="delta">Signed amount to add.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="EntityNotFoundException"></exception>
    public static async Task AddToBalanceAsync(this IEntityRepository<Account> accountRepository,
        Guid accountId, decimal delta, CancellationToken cancellationToken = default)
    {
        var found = await accountRepository.IncrementAsync(accountId, x => x.Balance, delta, cancellationToken);

        if (!found)
        {
            throw new EntityNotFoundException(typeof(Account));
        }
    }
}
