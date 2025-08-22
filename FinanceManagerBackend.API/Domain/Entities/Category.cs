namespace FinanceManagerBackend.API.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; }

    public string Emoji { get; set; }

    public bool IsIncome { get; set; }
}