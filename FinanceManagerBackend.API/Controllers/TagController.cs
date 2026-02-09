using FinanceManagerBackend.API.Models.Tags;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagerBackend.API.Controllers;

public class TagController : BaseController
{
    [HttpGet]
    public async Task<ActionResult<IList<TagResponse>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        return Ok(new List<TagResponse>());
    }
}