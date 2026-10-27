using System.Linq.Expressions;
using FinanceManagerBackend.API.Domain;
using FinanceManagerBackend.API.Domain.Entities;
using FinanceManagerBackend.API.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
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
        // Only identifiers from the EF model get into the SQL text, delimited by the provider;
        // all values are parameters. Nothing from the request is concatenated.
        var entityType = context.Model.FindEntityType(typeof(TEntity))!;
        var table = StoreObjectIdentifier.Table(entityType.GetTableName()!, entityType.GetSchema());
        var sqlHelper = context.GetService<ISqlGenerationHelper>();

        string Column(string propertyName) =>
            sqlHelper.DelimitIdentifier(entityType.FindProperty(propertyName)!.GetColumnName(table)!);

        // x => x.Property, where Property is mapped: the column name is taken from the model, not from the lambda.
        if (property.Body is not MemberExpression { Expression: ParameterExpression } member ||
            entityType.FindProperty(member.Member.Name) == null)
        {
            throw new ArgumentException("Expected a mapped property access like x => x.Balance", nameof(property));
        }

        var column = Column(member.Member.Name);

        // Single statement: runs inside the current DB transaction (if any) and takes a row lock until it ends.
        var affected = await context.Database.ExecuteSqlRawAsync(
            $$"""
              UPDATE {{sqlHelper.DelimitIdentifier(table.Name, table.Schema)}}
              SET {{column}} = {{column}} + {0}, {{Column(nameof(BaseEntity.UpdatedAt))}} = {1}
              WHERE {{Column(nameof(BaseEntity.Id))}} = {2}
              """,
            [delta, DateTimeOffset.UtcNow, id],
            cancellationToken);

        return affected > 0;
    }

    /// <inheritdoc />
    public async Task<IDbContextTransaction> BeginTransaction(CancellationToken cancellationToken = default)
    {
        return await context.Database.BeginTransactionAsync(cancellationToken);
    }
}