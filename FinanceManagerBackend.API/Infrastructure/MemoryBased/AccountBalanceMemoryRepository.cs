using FinanceManagerBackend.API.Domain;
using FinanceManagerBackend.API.Domain.Entities;

namespace FinanceManagerBackend.API.Infrastructure.MemoryBased;

/// <summary>
/// Memory based account balance repository implementation.
/// </summary>
/// <param name="accountRepository"></param>
public class AccountBalanceMemoryRepository(EntityMemoryRepository<Account> accountRepository) : IAccountBalanceRepository
{
    /// <inheritdoc />
    public async Task AddToBalanceAsync(Guid accountId, decimal delta, CancellationToken cancellationToken = default)
    {
        var account = await accountRepository.GetRequiredByIdAsync(accountId, cancellationToken);

        account.Balance += delta;
        account.UpdatedAt = DateTimeOffset.UtcNow;
    }
}
