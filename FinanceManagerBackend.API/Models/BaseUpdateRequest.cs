using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;

namespace FinanceManagerBackend.API.Models;

/// <summary>
/// Base update request.
/// </summary>
public class BaseUpdateRequest
{
    /// <summary>
    /// Updated at.
    /// </summary>
    [JsonIgnore]
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}