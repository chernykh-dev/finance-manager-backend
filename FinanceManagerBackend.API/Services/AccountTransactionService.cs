using FinanceManagerBackend.API.Domain;
using FinanceManagerBackend.API.Domain.Entities;

namespace FinanceManagerBackend.API.Services;

/// <inheritdoc />
public class AccountTransactionService(IAccountBalanceRepository accountBalanceRepository, IEntityRepository<Category> categoryRepository) : IAccountTransactionService
{
    /// <inheritdoc />
    public async Task UpdateAccountAmountAsync(Transaction transaction,
        CancellationToken cancellationToken = default)
    {
        var delta = await GetBalanceDeltaAsync(transaction, cancellationToken);

        await accountBalanceRepository.AddToBalanceAsync(transaction.AccountId, delta, cancellationToken);
    }

    /// <inheritdoc />
    public async Task ReplaceAccountAmountAsync(Transaction oldTransaction, Transaction updatedTransaction,
        CancellationToken cancellationToken = default)
    {
        await RollbackAccountAmountAsync(oldTransaction, cancellationToken);
        await UpdateAccountAmountAsync(updatedTransaction, cancellationToken);
    }

    /// <inheritdoc />
    public async Task RollbackAccountAmountAsync(Transaction transaction,
        CancellationToken cancellationToken = default)
    {
        var delta = await GetBalanceDeltaAsync(transaction, cancellationToken);

        await accountBalanceRepository.AddToBalanceAsync(transaction.AccountId, -delta, cancellationToken);
    }

    /// <summary>
    /// Returns signed balance change of transaction: +amount for income, -amount for expense.
    /// </summary>
    private async Task<decimal> GetBalanceDeltaAsync(Transaction transaction, CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetRequiredByIdReadonlyAsync(transaction.CategoryId, cancellationToken);

        return category.IsIncome ? transaction.Amount : -transaction.Amount;
    }
}