using System.ComponentModel.DataAnnotations;

namespace FinanceManagerBackend.API.Models.Users;

/// <summary>
/// User credentials model.
/// </summary>
public class UserCredentials
{
    /// <summary>
    /// User name.
    /// </summary>
    [Required]
    public string Name { get; set; } = null!;

    /// <summary>
    /// User password.
    /// </summary>
    [Required]
    public string Password { get; set; } = null!;
}