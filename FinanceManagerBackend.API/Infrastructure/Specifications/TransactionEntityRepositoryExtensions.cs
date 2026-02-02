using System.Collections.Immutable;
using FinanceManagerBackend.API.Domain;
using FinanceManagerBackend.API.Domain.Entities;

namespace FinanceManagerBackend.API.Infrastructure.Specifications;

/// <summary>
/// Extensions methods for IEntityRepository&lt;Transaction&gt;.
/// </summary>
public static class TransactionEntityRepositoryExtensions
{
    /// <summary>
    /// Returns read-only collection of transactions by accountId & period [startDate, endDate].
    /// </summary>
    /// <param name="transactionRepository"></param>
    /// <param name="accountId"></param>
    /// <param name="startDate">If null, startDate is min.</param>
    /// <param name="endDate">If null, endDate is max.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<IReadOnlyCollection<Transaction>> GetAccountTransactionsByPeriod(
        this IEntityRepository<Transaction> transactionRepository, Guid accountId,
        DateTimeOffset? startDate, DateTimeOffset? endDate,
        CancellationToken cancellationToken = default)
    {
        startDate = startDate ?? DateTimeOffset.MinValue;
        endDate = endDate ?? DateTimeOffset.MaxValue;

        var entities = await transactionRepository
            .GetAllByReadonlyAsync(x =>
                    x.AccountId == accountId &&
                    x.DateTime >= startDate && x.DateTime <= endDate,
                cancellationToken);

        return entities.ToImmutableList();
    }
}