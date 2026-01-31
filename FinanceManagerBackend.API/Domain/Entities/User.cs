using Microsoft.EntityFrameworkCore;

namespace FinanceManagerBackend.API.Domain.Entities;

/// <summary>
/// User entity.
/// </summary>
[Index(nameof(Name), IsUnique = true)]
public class User : BaseEntity
{
    /// <summary>
    /// User name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// User password.
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// User auth refresh token.
    /// </summary>
    public string RefreshToken { get; set; }

    /// <summary>
    /// User auth expired at.
    /// </summary>
    public DateTimeOffset ExpiredAt { get; set; }
}