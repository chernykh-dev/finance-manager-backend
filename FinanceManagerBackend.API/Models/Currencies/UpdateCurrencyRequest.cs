using System.ComponentModel.DataAnnotations;

namespace FinanceManagerBackend.API.Models.Currencies;

/// <summary>
/// Update currency request model.
/// </summary>
public class UpdateCurrencyRequest : BaseUpdateRequest
{
    /// <summary>
    /// Updated currency name.
    /// </summary>
    /// <example>RUB</example>
    [Required, MinLength(3), MaxLength(3)]
    public string Name { get; set; }
}