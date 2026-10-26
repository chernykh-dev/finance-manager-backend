using FinanceManagerBackend.API.Domain;
using FinanceManagerBackend.API.Domain.Entities;
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
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<TransactionResponse>> CreateAsync([FromBody] CreateTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = request.Adapt<Transaction>();

        await ThrowIfNotOwnAccountAsync(entity.AccountId, cancellationToken);

        await transactionCommonValidator.ValidateAndThrowAsync(entity, cancellationToken);

        var transaction = await transactionRepository.BeginTransaction(cancellationToken);

        if (entity.Id == Guid.Empty)
        {
            entity.Id = Guid.NewGuid();
        }
        await transactionRepository.CreateAsync(entity, cancellationToken);

        await accountTransactionService.UpdateAccountAmountAsync(entity, cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        var result = CreatedAtAction("Get", new { id = entity.Id }, entity.Adapt<TransactionResponse>());

        return result;
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

        // The transaction may be moved to another account: it must be the user's too.
        await ThrowIfNotOwnAccountAsync(updatedEntity.AccountId, cancellationToken);

        await transactionCommonValidator.ValidateAndThrowAsync(updatedEntity, cancellationToken);

        var transaction = await transactionRepository.BeginTransaction(cancellationToken);

        await accountTransactionService.ReplaceAccountAmountAsync(entity, updatedEntity, cancellationToken);
        await transactionRepository.UpdateAsync(updatedEntity, cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return Ok();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id,
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

        var transaction = await transactionRepository.BeginTransaction(cancellationToken);

        await accountTransactionService.RollbackAccountAmountAsync(entity, cancellationToken);
        await transactionRepository.DeleteAsync(entity, cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return Ok();
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