using FinanceManagerBackend.API.Domain;
using FinanceManagerBackend.API.Domain.Entities;

namespace FinanceManagerBackend.API.Services;

/// <inheritdoc />
public class AccountTransactionService(IEntityRepository<Account> accountRepository, IEntityRepository<Category> categoryRepository) : IAccountTransactionService
{
    /// <inheritdoc />
    public async Task UpdateAccountAmountAsync(Transaction transaction,
        CancellationToken cancellationToken = default)
    {
        var account = await accountRepository.GetRequiredByIdAsync(transaction.AccountId, cancellationToken);

        var category = await categoryRepository.GetRequiredByIdReadonlyAsync(transaction.CategoryId, cancellationToken);

        if (category.IsIncome)
        {
            account.Balance += transaction.Amount;
        }
        else
        {
            account.Balance -= transaction.Amount;
        }

        await accountRepository.UpdateAsync(account, cancellationToken);
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
        var account = await accountRepository.GetRequiredByIdAsync(transaction.AccountId, cancellationToken);
        var category = await categoryRepository.GetRequiredByIdReadonlyAsync(transaction.CategoryId, cancellationToken);

        if (category.IsIncome)
        {
            account.Balance -= transaction.Amount;
        }
        else
        {
            account.Balance += transaction.Amount;
        }

        await accountRepository.UpdateAsync(account, cancellationToken);
    }
}