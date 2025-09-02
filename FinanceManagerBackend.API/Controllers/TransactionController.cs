using FinanceManagerBackend.API.Domain;
using FinanceManagerBackend.API.Domain.Entities;
using FinanceManagerBackend.API.Models.Transactions;
using FluentValidation;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagerBackend.API.Controllers;

public class TransactionController(IEntityRepository<Transaction> transactionRepository, IValidator<Transaction> transactionCommonValidator) : BaseController
{
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

    [HttpPost]
    public async Task<ActionResult<TransactionResponse>> CreateAsync([FromBody] CreateTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = request.Adapt<Transaction>();

        await transactionCommonValidator.ValidateAndThrowAsync(entity, cancellationToken);

        entity.Id = Guid.NewGuid();
        await transactionRepository.CreateAsync(entity, cancellationToken);

        var result = CreatedAtAction("Get", new { id = entity.Id }, entity.Adapt<TransactionResponse>());

        return result;
    }

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