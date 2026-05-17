namespace FinanceManagerBackend.API.Domain.Entities;

public class Tag : BaseEntity
{
    public string Name { get; set; }

    public Guid ByUserId { get; set; }
}