using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace FinanceManagerBackend.API.Domain.Entities;

/// <summary>
/// Currency entity.
/// </summary>
[Index(nameof(Name), IsUnique = true)]
public class Currency : BaseEntity
{
    /// <summary>
    /// Currency name.
    /// </summary>
    [Required, MinLength(3), MaxLength(3)]
    public string Name { get; set; } = null!;
}