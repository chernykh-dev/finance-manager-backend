using FinanceManagerBackend.API.Domain;
using FinanceManagerBackend.API.Domain.Entities;
using FinanceManagerBackend.API.Domain.Entities.Enums;
using FinanceManagerBackend.API.Exceptions;
using FinanceManagerBackend.API.Infrastructure;
using FinanceManagerBackend.API.Models.Accounts;
using FinanceManagerBackend.API.Models.Transactions;
using FinanceManagerBackend.API.Services;
using FluentValidation;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagerBackend.API.Controllers;

/// <summary>
/// Account controller.
/// </summary>
/// <param name="accountRepository"></param>
/// <param name="accountCommonValidator"></param>
public class AccountController(
    IEntityRepository<Account> accountRepository, IEntityRepository<Transaction> transactionRepository,
    IValidator<Account> accountCommonValidator, IValidator<Transaction> transactionCommonValidator,
    IAccountTransactionService accountTransactionService)
    : BaseController
{
    /// <summary>
    /// Get all accounts.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<IList<AccountResponse>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();

        var entities =
            await accountRepository.GetAllByReadonlyAsync(x => x.UserId == userId && x.Status != EStatus.Deleted,
                cancellationToken);

        return Ok(entities.Adapt<List<AccountResponse>>());
    }

    /// <summary>
    /// Get account by id.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AccountResponse>> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await accountRepository.GetByIdReadonlyAsync(id, cancellationToken);

        if (entity == null)
        {
            return NotFound();
        }

        // Another user's account is reported exactly as a missing one.
        if (entity.UserId != GetUserId())
        {
            return NotFound();
        }

        return Ok(entity.Adapt<AccountResponse>());
    }

    /// <summary>
    /// Create account.
    /// Idempotent by id: a repeated request with an existing id and the same content returns
    /// the stored account (200); different content returns 409.
    /// 409 is also returned when another account of the user already has this name.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<AccountResponse>> CreateAsync([FromBody] CreateAccountRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();

        var entity = request.Adapt<Account>();

        if (entity.Id == Guid.Empty)
        {
            entity.Id = Guid.NewGuid();
        }
        entity.UserId = userId;

        // A savepoint when Idempotency-Key handling already opened a transaction: a failed insert rolls back
        // only this part and the rest of the request keeps working.
        await using var transaction = await accountRepository.BeginTransaction(cancellationToken);

        // Must go before the name check: a replay of this very request would otherwise
        // hit "name already taken" by the account it created itself.
        var existing = await accountRepository.GetByIdReadonlyAsync(entity.Id, cancellationToken);
        if (existing != null)
        {
            return Replay(existing, entity);
        }

        await accountCommonValidator.ValidateAndThrowAsync(entity, cancellationToken);

        var entityWithName =
            await accountRepository.GetByReadonlyAsync(x => x.Name == entity.Name && x.UserId == userId,
                cancellationToken);
        if (entityWithName != null)
        {
            // A concurrent duplicate of this very request created it after our id check: that's a replay.
            if (entityWithName.Id == entity.Id)
            {
                return Replay(entityWithName, entity);
            }

            return Problem(statusCode: StatusCodes.Status409Conflict,
                title: "Account with this name already exists");
        }

        try
        {
            await accountRepository.CreateAsync(entity, cancellationToken);
        }
        catch (EntityExistsException)
        {
            // A concurrent request with the same id won the insert: roll back and read its committed result.
            await transaction.RollbackAsync(cancellationToken);

            existing = await accountRepository.GetRequiredByIdReadonlyAsync(entity.Id, cancellationToken);

            return Replay(existing, entity);
        }

        await transaction.CommitAsync(cancellationToken);

        var result = CreatedAtAction("Get", new { id = entity.Id }, entity.Adapt<AccountResponse>());

        return result;
    }

    private ActionResult<AccountResponse> Replay(Account existing, Account requested)
    {
        if (!IsSameContent(existing, requested))
        {
            return Problem(statusCode: StatusCodes.Status409Conflict,
                title: "Account with this id already exists with different content");
        }

        return Ok(existing.Adapt<AccountResponse>());
    }

    private static bool IsSameContent(Account existing, Account requested)
    {
        // Another user's account is reported as a plain conflict, without its content.
        // Balance is not compared: transactions change it after creation, so a late replay would never match.
        return existing.UserId == requested.UserId &&
               existing.Name == requested.Name &&
               existing.CurrencyId == requested.CurrencyId;
    }

    /// <summary>
    /// Update account.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateAccountRequest request,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await accountRepository.BeginTransaction(cancellationToken);

        // The whole row is written back, so the balance must be current: the lock makes concurrent
        // transactions of this account (atomic balance UPDATE) wait instead of being overwritten.
        var entity = await accountRepository.GetByIdForUpdateAsync(id, cancellationToken);

        if (entity == null)
        {
            return NotFound();
        }

        if (entity.UserId != GetUserId())
        {
            return NotFound();
        }

        var updatedEntity = request.Adapt<Account>();
        updatedEntity.Id = id;
        // Not part of the request: keep the stored values instead of the entity defaults.
        updatedEntity.UserId = entity.UserId;
        updatedEntity.Status = entity.Status;
        updatedEntity.CreatedAt = entity.CreatedAt;
        // Balance changes only through transactions and balance adjustments.
        updatedEntity.Balance = entity.Balance;

        await accountCommonValidator.ValidateAndThrowAsync(updatedEntity, cancellationToken);

        await accountRepository.UpdateAsync(updatedEntity, cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return Ok();
    }

    /// <summary>
    /// Manual balance adjustment: set the account balance to the given value.
    /// The difference with the current balance is recorded as a transaction in the system
    /// "Корректировка" category (income if the balance grows, expense otherwise), so it shows up in history
    /// and is consistent with concurrent transactions. Nothing is created if the balance is already equal.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("{id:guid}/balance-adjustment")]
    public async Task<ActionResult<BalanceAdjustmentResponse>> AdjustBalanceAsync(Guid id,
        [FromBody] BalanceAdjustmentRequest request, CancellationToken cancellationToken = default)
    {
        await using var transaction = await accountRepository.BeginTransaction(cancellationToken);

        // Locked until commit: the difference is computed from the current balance and no other
        // transaction of this account can change it in between.
        var account = await accountRepository.GetByIdForUpdateAsync(id, cancellationToken);

        if (account == null || account.UserId != GetUserId())
        {
            return NotFound();
        }

        var delta = request.Balance - account.Balance;

        TransactionResponse? adjustment = null;

        if (delta != 0)
        {
            var entity = new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = account.Id,
                CategoryId = Guid.Parse(delta > 0
                    ? DbInitializer.BalanceAdjustmentIncomeCategoryId
                    : DbInitializer.BalanceAdjustmentExpenseCategoryId),
                Amount = Math.Abs(delta),
                DateTime = request.DateTime ?? DateTimeOffset.UtcNow,
                Comment = request.Comment
            };

            // Same rules as a regular transaction (e.g. a deleted account can't be adjusted).
            await transactionCommonValidator.ValidateAndThrowAsync(entity, cancellationToken);

            await transactionRepository.CreateAsync(entity, cancellationToken);
            await accountTransactionService.UpdateAccountAmountAsync(entity, cancellationToken);

            adjustment = entity.Adapt<TransactionResponse>();
        }

        await transaction.CommitAsync(cancellationToken);

        var updatedAccount = await accountRepository.GetRequiredByIdReadonlyAsync(id, cancellationToken);

        return Ok(new BalanceAdjustmentResponse
        {
            Account = updatedAccount.Adapt<AccountResponse>(),
            Transaction = adjustment
        });
    }

    /// <summary>
    /// Delete account.
    /// Idempotent: deleting a missing account is a success (204).
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await accountRepository.GetByIdAsync(id, cancellationToken);

        if (entity == null)
        {
            return NoContent();
        }

        // Another user's account is reported exactly as a missing one.
        if (entity.UserId != GetUserId())
        {
            return NoContent();
        }

        var firstTransaction = await transactionRepository.GetByReadonlyAsync(x => x.AccountId == id,
            cancellationToken);

        if (firstTransaction == null)
        {
            await accountRepository.DeleteAsync(entity, cancellationToken);
        }
        else
        {
            entity.Status = EStatus.Deleted;

            await accountRepository.UpdateAsync(entity, cancellationToken);
        }

        return NoContent();
    }
}