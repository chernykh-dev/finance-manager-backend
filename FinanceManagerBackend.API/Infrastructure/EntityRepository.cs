using System.Linq.Expressions;
using FinanceManagerBackend.API.Domain;
using FinanceManagerBackend.API.Domain.Entities;
using FinanceManagerBackend.API.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;

namespace FinanceManagerBackend.API.Infrastructure;

/// <inheritdoc />
public class EntityRepository<TEntity>(ServiceDbContext context, ILogger<EntityRepository<TEntity>> logger) : IEntityRepository<TEntity>
    where TEntity : BaseEntity
{
    /// <inheritdoc />
    public async Task<IEnumerable<TEntity>> GetAllReadonlyAsync(CancellationToken cancellationToken = default)
    {
        return await context
            .Set<TEntity>()
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TEntity>> GetAllByReadonlyAsync(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await context
            .Set<TEntity>()
            .Where(predicate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TEntity?> GetByIdReadonlyAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context
            .Set<TEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context
            .Set<TEntity>()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TEntity> GetRequiredByIdReadonlyAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await GetByIdReadonlyAsync(id, cancellationToken);

        if (result == null)
        {
            throw new EntityNotFoundException(typeof(TEntity));
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<TEntity> GetRequiredByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await GetByIdAsync(id, cancellationToken);

        if (result == null)
        {
            throw new EntityNotFoundException(typeof(TEntity));
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<TEntity?> GetByReadonlyAsync(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await context
            .Set<TEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(predicate, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TEntity?> GetByAsync(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await context
            .Set<TEntity>()
            .FirstOrDefaultAsync(predicate, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var exists = await context
            .Set<TEntity>()
            .AsNoTracking()
            .AnyAsync(e => e.Id == entity.Id, cancellationToken);

        if (exists)
        {
            throw new EntityExistsException(typeof(TEntity));
        }

        var entityEntry = await context
            .Set<TEntity>()
            .AddAsync(entity, cancellationToken);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsPrimaryKeyViolation(exception))
        {
            // The check above is only a fast path: a concurrent request with the same id
            // can insert between AnyAsync and SaveChanges. The primary key is the real guard.
            entityEntry.State = EntityState.Detached;

            throw new EntityExistsException(typeof(TEntity));
        }

        return entityEntry.Entity;
    }

    /// <inheritdoc />
    public async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var exists = await context
            .Set<TEntity>()
            .AsNoTracking()
            .AnyAsync(e => e.Id == entity.Id, cancellationToken);

        if (!exists)
        {
            throw new EntityNotFoundException(typeof(TEntity));
        }

        context.Set<TEntity>().Update(entity);

        await context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var exists = await context
            .Set<TEntity>()
            .AsNoTracking()
            .AnyAsync(e => e.Id == entity.Id, cancellationToken);

        if (!exists)
        {
            throw new EntityNotFoundException(typeof(TEntity));
        }

        context.Set<TEntity>().Remove(entity);

        await context.SaveChangesAsync(cancellationToken);
    }

    private bool IsPrimaryKeyViolation(DbUpdateException exception)
    {
        var primaryKeyName = context.Model.FindEntityType(typeof(TEntity))?.FindPrimaryKey()?.GetName();

        return exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation
        } postgresException && postgresException.ConstraintName == primaryKeyName;
    }

    /// <inheritdoc />
    public async Task<IDbContextTransaction> BeginTransaction(CancellationToken cancellationToken = default)
    {
        return await context.Database.BeginTransactionAsync(cancellationToken);
    }
}