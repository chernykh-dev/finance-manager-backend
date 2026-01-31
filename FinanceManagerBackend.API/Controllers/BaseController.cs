using Asp.Versioning;
using FinanceManagerBackend.API.HttpPipelines;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagerBackend.API.Controllers;

/// <summary>
/// Base class for controllers.
/// </summary>
[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/[controller]")]
public class BaseController : ControllerBase
{
    /// <summary>
    /// Returns current user id.
    /// </summary>
    /// <returns></returns>
    protected Guid GetUserId()
    {
        return this.GetUserIdFromClaims();
    }
}