namespace FinanceManagerBackend.API.Models;

public class BaseUpdateRequest
{
    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}