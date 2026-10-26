using FinanceManagerBackend.API.Domain;
using FinanceManagerBackend.API.Domain.Entities;
using FinanceManagerBackend.API.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace FinanceManagerBackend.API.Infrastructure;

/// <inheritdoc />
public class AccountBalanceRepository(ServiceDbContext context) : IAccountBalanceRepository
{
    /// <inheritdoc />
    public async Task AddToBalanceAsync(Guid accountId, decimal delta, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        // UPDATE accounts SET balance = balance + @delta ... WHERE id = @id
        // Runs inside the current DB transaction (if any) and takes a row lock until it ends.
        var affected = await context.Accounts
            .Where(x => x.Id == accountId)
            .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Balance, x => x.Balance + delta)
                    .SetProperty(x => x.UpdatedAt, now),
                cancellationToken);

        if (affected == 0)
        {
            throw new EntityNotFoundException(typeof(Account));
        }
    }
}
