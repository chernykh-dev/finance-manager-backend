using Asp.Versioning;
using FinanceManagerBackend.API.Domain;
using FinanceManagerBackend.API.Domain.Entities;
using FinanceManagerBackend.API.Models.Currencies;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace FinanceManagerBackend.API.Controllers;

public class CurrencyController(IEntityRepository<Currency> currencyRepository) : BaseController
{
    [HttpGet]
    public async Task<ActionResult<IList<CurrencyResponse>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await currencyRepository.GetAllReadonlyAsync(cancellationToken);

        return Ok(entities.Adapt<List<CurrencyResponse>>());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CurrencyResponse>> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await currencyRepository.GetByIdReadonlyAsync(id, cancellationToken);

        if (entity == null)
        {
            return NotFound();
        }

        return Ok(entity.Adapt<CurrencyResponse>());
    }

    [HttpPost]
    public async Task<ActionResult<CurrencyResponse>> CreateAsync([FromBody] CreateCurrencyRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = request.Adapt<Currency>();
        entity.Id = Guid.NewGuid();

        try
        {
            await currencyRepository.CreateAsync(entity, cancellationToken);
        }
        catch (DbUpdateException exception)
        {
            if (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                return Conflict();
            }
        }

        var result = CreatedAtAction("Get", new { id = entity.Id }, entity.Adapt<CurrencyResponse>());

        return result;
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateCurrencyRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await currencyRepository.GetByIdReadonlyAsync(id, cancellationToken);

        if (entity == null)
        {
            return NotFound();
        }

        var updatedEntity = request.Adapt<Currency>();
        updatedEntity.Id = id;

        await currencyRepository.UpdateAsync(updatedEntity, cancellationToken);

        return Ok();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await currencyRepository.GetByIdReadonlyAsync(id, cancellationToken);

        if (entity == null)
        {
            return NotFound();
        }

        await currencyRepository.DeleteAsync(entity, cancellationToken);

        return Ok();
    }
}