using Microsoft.EntityFrameworkCore.Storage;

namespace FinanceManagerBackend.API.Infrastructure;

/// <summary>
/// Nested transaction on top of an already started one, backed by a savepoint:
/// commit releases the savepoint, rollback (or dispose without commit) rolls back to it.
/// The outer transaction stays in charge of the real commit.
/// </summary>
public class SavepointDbContextTransaction : IDbContextTransaction
{
    private readonly IDbContextTransaction _outerTransaction;
    private readonly string _savepointName = $"sp_{Guid.NewGuid():N}";
    private bool _completed;

    private SavepointDbContextTransaction(IDbContextTransaction outerTransaction)
    {
        _outerTransaction = outerTransaction;
    }

    /// <summary>
    /// Creates savepoint in outer transaction.
    /// </summary>
    /// <param name="outerTransaction"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<SavepointDbContextTransaction> CreateAsync(IDbContextTransaction outerTransaction,
        CancellationToken cancellationToken = default)
    {
        var transaction = new SavepointDbContextTransaction(outerTransaction);

        await outerTransaction.CreateSavepointAsync(transaction._savepointName, cancellationToken);

        return transaction;
    }

    /// <inheritdoc />
    public Guid TransactionId => _outerTransaction.TransactionId;

    /// <inheritdoc />
    public void Commit()
    {
        _outerTransaction.ReleaseSavepoint(_savepointName);
        _completed = true;
    }

    /// <inheritdoc />
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await _outerTransaction.ReleaseSavepointAsync(_savepointName, cancellationToken);
        _completed = true;
    }

    /// <inheritdoc />
    public void Rollback()
    {
        _outerTransaction.RollbackToSavepoint(_savepointName);
        _completed = true;
    }

    /// <inheritdoc />
    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        await _outerTransaction.RollbackToSavepointAsync(_savepointName, cancellationToken);
        _completed = true;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (!_completed)
        {
            Rollback();
        }

        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (!_completed)
        {
            await RollbackAsync();
        }

        GC.SuppressFinalize(this);
    }
}
