using System.Linq.Expressions;
using FinanceManagerBackend.API.Domain.Entities;
using FinanceManagerBackend.API.Exceptions;
using Microsoft.EntityFrameworkCore.Storage;

namespace FinanceManagerBackend.API.Domain;

/// <summary>
/// Entity repository for entities inherited from BaseEntity.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public interface IEntityRepository<TEntity>
    where TEntity : BaseEntity
{
    /// <summary>
    /// Get all entities readonly.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<TEntity>> GetAllReadonlyAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all entities by predicate readonly.
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<TEntity>> GetAllByReadonlyAsync(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get single entity by id readonly.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<TEntity?> GetByIdReadonlyAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get single entity by id.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get required single entity by id readonly.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="EntityNotFoundException"></exception>
    Task<TEntity> GetRequiredByIdReadonlyAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get required single entity by id.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="EntityNotFoundException"></exception>
    Task<TEntity> GetRequiredByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get single entity by predicate readonly.
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<TEntity?> GetByReadonlyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get single by predicate.
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<TEntity?> GetByAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create new entity.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="EntityExistsException"></exception>
    Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update entity.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="EntityNotFoundException"></exception>
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete entity.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="EntityNotFoundException"></exception>
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns new started transaction.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IDbContextTransaction> BeginTransaction(CancellationToken cancellationToken = default);
}