using FinanceManagerBackend.API.Domain;
using FinanceManagerBackend.API.Domain.Entities;
using FinanceManagerBackend.API.Exceptions;
using FinanceManagerBackend.API.Infrastructure.Specifications;
using FinanceManagerBackend.API.Models.Transactions;
using FinanceManagerBackend.API.Services;
using FluentValidation;
using FluentValidation.Results;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagerBackend.API.Controllers;

/// <summary>
/// Transaction controller.
/// </summary>
/// <param name="transactionRepository"></param>
/// <param name="accountRepository"></param>
/// <param name="transactionCommonValidator"></param>
/// <param name="accountTransactionService"></param>
public class TransactionController(
    IEntityRepository<Transaction> transactionRepository,
    IEntityRepository<Account> accountRepository,
    IValidator<Transaction> transactionCommonValidator,
    IAccountTransactionService accountTransactionService
    ) : BaseController
{
    /// <summary>
    /// Get transaction by id.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TransactionResponse>> GetAsync(Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await transactionRepository.GetByIdReadonlyAsync(id, cancellationToken);

        if (entity == null)
        {
            return NotFound();
        }

        // Transaction belongs to the owner of its account; another user's one is reported as missing.
        if (!await IsOwnAccountAsync(entity.AccountId, cancellationToken))
        {
            return NotFound();
        }

        return Ok(entity);
    }

    /// <summary>
    /// Get account transactions by period [startDate, endDate].
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="startDate">If null, startDate is min.</param>
    /// <param name="endDate">If null, startDate is max.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("{accountId:guid}/period")]
    public async Task<ActionResult<List<TransactionResponse>>> GetAccountTransactionsByPeriodAsync(Guid accountId,
        [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate,
        CancellationToken cancellationToken = default)
    {
        if (!await IsOwnAccountAsync(accountId, cancellationToken))
        {
            return NotFound();
        }

        var entities =
            await transactionRepository.GetAccountTransactionsByPeriod(accountId, startDate, endDate,
                cancellationToken);

        return Ok(entities);
    }

    /// <summary>
    /// Create new transaction.
    /// Idempotent by id: a repeated request with an existing id and the same content returns
    /// the stored transaction (200) without applying the amount again; different content returns 409.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<TransactionResponse>> CreateAsync([FromBody] CreateTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = request.Adapt<Transaction>();

        // Must go before the replay check: a replay must never return another user's transaction.
        await ThrowIfNotOwnAccountAsync(entity.AccountId, cancellationToken);

        if (entity.Id == Guid.Empty)
        {
            entity.Id = Guid.NewGuid();
        }

        await using var transaction = await transactionRepository.BeginTransaction(cancellationToken);

        // Replay check goes first and must return before UpdateAccountAmountAsync:
        // the balance is changed by delta, applying it twice corrupts the account.
        var existing = await transactionRepository.GetByIdReadonlyAsync(entity.Id, cancellationToken);
        if (existing != null)
        {
            return Replay(existing, entity);
        }

        await transactionCommonValidator.ValidateAndThrowAsync(entity, cancellationToken);

        try
        {
            await transactionRepository.CreateAsync(entity, cancellationToken);
        }
        catch (EntityExistsException)
        {
            // A concurrent request with the same id won the insert. The DB transaction is aborted,
            // so roll back and read its committed result.
            await transaction.RollbackAsync(cancellationToken);

            existing = await transactionRepository.GetRequiredByIdReadonlyAsync(entity.Id, cancellationToken);

            return Replay(existing, entity);
        }

        await accountTransactionService.UpdateAccountAmountAsync(entity, cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        var result = CreatedAtAction("Get", new { id = entity.Id }, entity.Adapt<TransactionResponse>());

        return result;
    }

    private ActionResult<TransactionResponse> Replay(Transaction existing, Transaction requested)
    {
        if (!IsSameContent(existing, requested))
        {
            return Problem(statusCode: StatusCodes.Status409Conflict,
                title: "Transaction with this id already exists with different content");
        }

        return Ok(existing.Adapt<TransactionResponse>());
    }

    private static bool IsSameContent(Transaction existing, Transaction requested)
    {
        return existing.AccountId == requested.AccountId &&
               existing.CategoryId == requested.CategoryId &&
               existing.Amount == requested.Amount &&
               // PostgreSQL stores timestamps with microsecond precision.
               existing.DateTime.UtcTicks / 10 == requested.DateTime.UtcTicks / 10 &&
               existing.Comment == requested.Comment;
    }

    /// <summary>
    /// Update transaction.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await transactionRepository.GetByIdReadonlyAsync(id, cancellationToken);

        if (entity == null)
        {
            return NotFound();
        }

        if (!await IsOwnAccountAsync(entity.AccountId, cancellationToken))
        {
            return NotFound();
        }

        var updatedEntity = request.Adapt<Transaction>();
        updatedEntity.Id = id;
        // Not part of the request: keep the stored value instead of the entity default (now).
        updatedEntity.CreatedAt = entity.CreatedAt;

        // The transaction may be moved to another account: it must be the user's too.
        await ThrowIfNotOwnAccountAsync(updatedEntity.AccountId, cancellationToken);

        await transactionCommonValidator.ValidateAndThrowAsync(updatedEntity, cancellationToken);

        await using var transaction = await transactionRepository.BeginTransaction(cancellationToken);

        await accountTransactionService.ReplaceAccountAmountAsync(entity, updatedEntity, cancellationToken);
        await transactionRepository.UpdateAsync(updatedEntity, cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return Ok();
    }

    /// <summary>
    /// Delete transaction.
    /// Idempotent: deleting a missing transaction is a success (204).
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await transactionRepository.GetByIdReadonlyAsync(id, cancellationToken);

        if (entity == null)
        {
            return NoContent();
        }

        // Another user's transaction is reported exactly as a missing one.
        if (!await IsOwnAccountAsync(entity.AccountId, cancellationToken))
        {
            return NoContent();
        }

        await using var transaction = await transactionRepository.BeginTransaction(cancellationToken);

        await accountTransactionService.RollbackAccountAmountAsync(entity, cancellationToken);
        await transactionRepository.DeleteAsync(entity, cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return NoContent();
    }

    private async Task<bool> IsOwnAccountAsync(Guid accountId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();

        var account = await accountRepository.GetByReadonlyAsync(x => x.Id == accountId && x.UserId == userId,
            cancellationToken);

        return account != null;
    }

    /// <summary>
    /// Another user's account is reported exactly like a missing one: the same 400 TransactionValidator returns.
    /// </summary>
    private async Task ThrowIfNotOwnAccountAsync(Guid accountId, CancellationToken cancellationToken)
    {
        if (await IsOwnAccountAsync(accountId, cancellationToken))
        {
            return;
        }

        throw new ValidationException([
            new ValidationFailure(nameof(Transaction.AccountId), $"Account with id={accountId} not found")
        ]);
    }
}