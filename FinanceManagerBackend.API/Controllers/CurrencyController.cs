using Asp.Versioning;
using FinanceManagerBackend.API.Domain;
using FinanceManagerBackend.API.Domain.Entities;
using FinanceManagerBackend.API.Models.Currencies;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace FinanceManagerBackend.API.Controllers;

/// <summary>
/// Currency controller.
/// </summary>
/// <param name="currencyRepository"></param>
public class CurrencyController(IEntityRepository<Currency> currencyRepository) : BaseController
{
    /// <summary>
    /// Get all currencies.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<IList<CurrencyResponse>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();

        var entities = await currencyRepository.GetAllReadonlyAsync(cancellationToken);

        return Ok(entities.Adapt<List<CurrencyResponse>>());
    }

    /// <summary>
    /// Get currency by id.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Create new currency.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<CurrencyResponse>> CreateAsync([FromBody] CreateCurrencyRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = request.Adapt<Currency>();

        var entityWithName = await currencyRepository.GetByReadonlyAsync(x => x.Name == entity.Name, cancellationToken);
        if (entityWithName != null)
        {
            return Conflict();
        }

        entity.Id = Guid.NewGuid();
        await currencyRepository.CreateAsync(entity, cancellationToken);

        var result = CreatedAtAction("Get", new { id = entity.Id }, entity.Adapt<CurrencyResponse>());

        return result;
    }

    /// <summary>
    /// Update currency.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Delete currency.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
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