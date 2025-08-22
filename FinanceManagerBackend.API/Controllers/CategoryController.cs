using FinanceManagerBackend.API.Domain;
using FinanceManagerBackend.API.Domain.Entities;
using FinanceManagerBackend.API.Models.Currencies;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagerBackend.API.Controllers;

public class CategoryController(IEntityRepository<Category> categoryRepository) : BaseController
{
    [HttpGet]
    public async Task<ActionResult<IList<Category>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities =  await categoryRepository.GetAllReadonlyAsync(cancellationToken);

        return Ok(entities);
    }

    [HttpGet("type/{isIncome:bool}")]
    public async Task<ActionResult<IList<CurrencyResponse>>> GetAllByTypeAsync(bool isIncome,
        CancellationToken cancellationToken = default)
    {
        var entities = await categoryRepository
            .GetAllByReadonlyAsync(x => x.IsIncome == isIncome, cancellationToken);

        return Ok(entities);
    }
}