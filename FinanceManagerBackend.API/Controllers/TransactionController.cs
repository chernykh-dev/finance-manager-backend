using FinanceManagerBackend.API.Domain;
using FinanceManagerBackend.API.Domain.Entities;
using FinanceManagerBackend.API.Models.Transactions;
using FinanceManagerBackend.API.Services;
using FluentValidation;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagerBackend.API.Controllers;

/// <summary>
/// Transaction controller.
/// </summary>
/// <param name="transactionRepository"></param>
/// <param name="transactionCommonValidator"></param>
/// <param name="accountTransactionService"></param>
public class TransactionController(
    IEntityRepository<Transaction> transactionRepository,
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

        return Ok(entity);
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

        await transactionCommonValidator.ValidateAndThrowAsync(entity, cancellationToken);

        var transaction = await transactionRepository.BeginTransaction(cancellationToken);

        entity.Id = Guid.NewGuid();
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

        var updatedEntity = request.Adapt<Transaction>();
        updatedEntity.Id = id;

        await transactionCommonValidator.ValidateAndThrowAsync(updatedEntity, cancellationToken);

        await transactionRepository.UpdateAsync(updatedEntity, cancellationToken);

        return Ok();
    }
}