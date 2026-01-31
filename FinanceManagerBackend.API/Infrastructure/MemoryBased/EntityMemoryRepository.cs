using System.Linq.Expressions;
using FinanceManagerBackend.API.Domain;
using FinanceManagerBackend.API.Domain.Entities;
using FinanceManagerBackend.API.Exceptions;
using Microsoft.EntityFrameworkCore.Storage;

namespace FinanceManagerBackend.API.Infrastructure.MemoryBased;

/// <summary>
/// Memory based entity repository implementation (dictionary).
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public class EntityMemoryRepository<TEntity> : IEntityRepository<TEntity>, IDisposable
    where TEntity : BaseEntity
{
    private Dictionary<Guid, TEntity> _entities = new();

    /// <inheritdoc />
    public async Task<IEnumerable<TEntity>> GetAllReadonlyAsync(CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(_entities.Values);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TEntity>> GetAllByReadonlyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(_entities.Values.Where(predicate.Compile()));
    }

    /// <inheritdoc />
    public async Task<TEntity?> GetByIdReadonlyAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await GetByIdAsync(id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(_entities[id]);
    }

    /// <inheritdoc />
    public async Task<TEntity> GetRequiredByIdReadonlyAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await GetRequiredByIdAsync(id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TEntity> GetRequiredByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);

        return entity ?? throw new EntityNotFoundException(typeof(TEntity));
    }

    /// <inheritdoc />
    public async Task<TEntity?> GetByReadonlyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await GetByAsync(predicate, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TEntity?> GetByAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(_entities.Values.FirstOrDefault(predicate.Compile()));
    }

    /// <inheritdoc />
    public async Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (!_entities.TryAdd(entity.Id, entity))
        {
            throw new EntityExistsException(typeof(TEntity));
        }

        return await Task.FromResult(entity);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (!_entities.ContainsKey(entity.Id))
        {
            throw new EntityNotFoundException(typeof(TEntity));
        }

        _entities[entity.Id] = entity;

        await Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (!_entities.Remove(entity.Id))
        {
            throw new EntityNotFoundException(typeof(TEntity));
        }

        await Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<IDbContextTransaction> BeginTransaction(CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(new MemoryDbContextTransaction<TEntity>(this));
    }

    /// <summary>
    /// Returns entities dictionary copy.
    /// </summary>
    /// <returns></returns>
    public Dictionary<Guid, TEntity> GetMemoryCopy()
    {
        return new Dictionary<Guid, TEntity>(_entities);
    }

    /// <summary>
    /// Restores entities from copy.
    /// </summary>
    /// <param name="memoryCopy"></param>
    public void RestoreMemory(Dictionary<Guid, TEntity> memoryCopy)
    {
        _entities = memoryCopy;
    }

    ///<inheritdoc />
    public void Dispose()
    {
        _entities.Clear();

        GC.SuppressFinalize(this);
    }
}