using System.Linq.Expressions;
using System.Reflection;
using FinanceManagerBackend.API.Domain;
using FinanceManagerBackend.API.Domain.Entities;
using FinanceManagerBackend.API.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;

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

        await context.SaveChangesAsync(cancellationToken);

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

    /// <inheritdoc />
    public async Task<bool> IncrementAsync(Guid id, Expression<Func<TEntity, decimal>> property, decimal delta,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        // UPDATE ... SET property = property + @delta, updated_at = @now WHERE id = @id
        // Runs inside the current DB transaction (if any) and takes a row lock until it ends.
        var affected = await context
            .Set<TEntity>()
            .Where(e => e.Id == id)
            .ExecuteUpdateAsync(BuildIncrementSetters(property, delta, now), cancellationToken);

        return affected > 0;
    }

    /// <summary>
    /// Builds s => s.SetProperty(property, x => property(x) + delta).SetProperty(x => x.UpdatedAt, x => now).
    /// Written by hand because the property is a parameter, and ExecuteUpdate takes setters only as an expression tree.
    /// </summary>
    private static Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> BuildIncrementSetters(
        Expression<Func<TEntity, decimal>> property, decimal delta, DateTimeOffset now)
    {
        // Captured like a closure variable, so EF sends delta as a query parameter rather than a SQL literal.
        Expression<Func<decimal>> deltaValue = () => delta;
        var incremented = Expression.Lambda<Func<TEntity, decimal>>(
            Expression.Add(property.Body, deltaValue.Body), property.Parameters);

        Expression<Func<TEntity, DateTimeOffset>> updatedAt = x => x.UpdatedAt;
        Expression<Func<TEntity, DateTimeOffset>> updatedAtValue = x => now;

        var setters = Expression.Parameter(typeof(SetPropertyCalls<TEntity>), "s");

        var body = Expression.Call(
            Expression.Call(setters, SetPropertyMethod<decimal>(), property, incremented),
            SetPropertyMethod<DateTimeOffset>(), updatedAt, updatedAtValue);

        return Expression.Lambda<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>>(body, setters);
    }

    private static MethodInfo SetPropertyMethod<TProperty>()
    {
        // SetProperty<TProperty>(Func<TEntity, TProperty> propertyExpression, Func<TEntity, TProperty> valueExpression)
        return typeof(SetPropertyCalls<TEntity>)
            .GetMethods()
            .Single(x => x.Name == nameof(SetPropertyCalls<TEntity>.SetProperty) &&
                         x.GetParameters()[1].ParameterType.IsGenericType &&
                         x.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Func<,>))
            .MakeGenericMethod(typeof(TProperty));
    }

    /// <inheritdoc />
    public async Task<IDbContextTransaction> BeginTransaction(CancellationToken cancellationToken = default)
    {
        return await context.Database.BeginTransactionAsync(cancellationToken);
    }
}