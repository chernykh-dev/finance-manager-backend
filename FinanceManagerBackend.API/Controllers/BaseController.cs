using Asp.Versioning;
using FinanceManagerBackend.API.HttpPipelines;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagerBackend.API.Controllers;

[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/[controller]")]
public class BaseController : ControllerBase
{
    protected Guid GetUserId()
    {
        return this.GetUserIdFromClaims();
    }
}