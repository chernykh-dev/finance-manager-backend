namespace FinanceManagerBackend.API.Domain.Entities;

/// <summary>
/// Category entity.
/// </summary>
public class Category : BaseEntity
{
    /// <summary>
    /// Category name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Category icon (emoji).
    /// </summary>
    public string Emoji { get; set; }

    /// <summary>
    /// Category type.
    /// </summary>
    public bool IsIncome { get; set; }
}