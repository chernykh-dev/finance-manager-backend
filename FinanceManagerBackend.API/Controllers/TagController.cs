using FinanceManagerBackend.API.Domain;
using FinanceManagerBackend.API.Domain.Entities;
using FinanceManagerBackend.API.Models.Tags;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagerBackend.API.Controllers;

public class TagController(IEntityRepository<Tag> tagRepository) : BaseController
{
    [HttpGet]
    public async Task<ActionResult<IList<TagResponse>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await tagRepository.GetAllReadonlyAsync(cancellationToken);

        return Ok(entities);
    }
}