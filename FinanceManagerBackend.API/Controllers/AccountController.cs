using FinanceManagerBackend.API.Domain;
using FinanceManagerBackend.API.Domain.Entities;
using FinanceManagerBackend.API.Domain.Entities.Enums;
using FinanceManagerBackend.API.Exceptions;
using FinanceManagerBackend.API.Models.Accounts;
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
    IValidator<Account> accountCommonValidator)
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
            return Problem(statusCode: StatusCodes.Status409Conflict,
                title: "Account with this name already exists");
        }

        try
        {
            await accountRepository.CreateAsync(entity, cancellationToken);
        }
        catch (EntityExistsException)
        {
            // A concurrent request with the same id won the insert.
            existing = await accountRepository.GetRequiredByIdReadonlyAsync(entity.Id, cancellationToken);

            return Replay(existing, entity);
        }

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
        return existing.UserId == requested.UserId &&
               existing.Name == requested.Name &&
               existing.CurrencyId == requested.CurrencyId &&
               existing.Balance == requested.Balance;
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
        var entity = await accountRepository.GetByIdReadonlyAsync(id, cancellationToken);

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

        await accountCommonValidator.ValidateAndThrowAsync(updatedEntity, cancellationToken);

        await accountRepository.UpdateAsync(updatedEntity, cancellationToken);

        return Ok();
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