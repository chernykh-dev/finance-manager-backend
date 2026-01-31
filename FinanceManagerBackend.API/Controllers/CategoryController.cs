using FinanceManagerBackend.API.Domain;
using FinanceManagerBackend.API.Domain.Entities;
using FinanceManagerBackend.API.Models.Currencies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagerBackend.API.Controllers;

/// <summary>
/// Category controller.
/// </summary>
/// <param name="categoryRepository"></param>
public class CategoryController(IEntityRepository<Category> categoryRepository) : BaseController
{
    /// <summary>
    /// Get all categories.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<IList<Category>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities =  await categoryRepository.GetAllReadonlyAsync(cancellationToken);

        return Ok(entities);
    }

    /// <summary>
    /// Get all categories by type.
    /// </summary>
    /// <param name="isIncome"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("type/{isIncome:bool}")]
    public async Task<ActionResult<IList<CurrencyResponse>>> GetAllByTypeAsync(bool isIncome,
        CancellationToken cancellationToken = default)
    {
        var entities = await categoryRepository
            .GetAllByReadonlyAsync(x => x.IsIncome == isIncome, cancellationToken);

        return Ok(entities);
    }
}