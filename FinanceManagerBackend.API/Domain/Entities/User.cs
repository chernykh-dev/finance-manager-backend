using Microsoft.EntityFrameworkCore;

namespace FinanceManagerBackend.API.Domain.Entities;

[Index(nameof(Name), IsUnique = true)]
public class User : BaseEntity
{
    public string Name { get; set; }

    public string Password { get; set; }

    public string RefreshToken { get; set; }

    public DateTimeOffset ExpiredAt { get; set; }
}