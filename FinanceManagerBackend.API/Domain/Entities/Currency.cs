using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace FinanceManagerBackend.API.Domain.Entities;

[Index(nameof(Name), IsUnique = true)]
public class Currency : BaseEntity
{
    [Required, MinLength(3), MaxLength(3)]
    public string Name { get; set; } = null!;
}