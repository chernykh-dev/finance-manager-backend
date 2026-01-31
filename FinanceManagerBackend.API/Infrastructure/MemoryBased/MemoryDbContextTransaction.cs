using FinanceManagerBackend.API.Domain;
using FinanceManagerBackend.API.Domain.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace FinanceManagerBackend.API.Infrastructure.MemoryBased;

/// <summary>
/// Memory database transaction.
/// </summary>
/// <param name="entityMemoryRepository"></param>
/// <typeparam name="TEntity"></typeparam>
public class MemoryDbContextTransaction<TEntity>(
    EntityMemoryRepository<TEntity> entityMemoryRepository
) : IDbContextTransaction
    where TEntity : BaseEntity
{
    private readonly Dictionary<Guid, TEntity> _memoryCopy = entityMemoryRepository.GetMemoryCopy();

    /// <inheritdoc />
    public void Commit()
    {

    }

    /// <inheritdoc />
    public async Task CommitAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        Commit();

        await Task.CompletedTask;
    }

    /// <inheritdoc />
    public void Rollback()
    {
        entityMemoryRepository.RestoreMemory(_memoryCopy);
    }

    /// <inheritdoc />
    public async Task RollbackAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        Rollback();

        await Task.CompletedTask;
    }

    /// <inheritdoc />
    public Guid TransactionId => Guid.NewGuid();

    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        await Task.CompletedTask;
    }
}