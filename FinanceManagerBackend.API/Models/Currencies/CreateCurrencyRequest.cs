using System.ComponentModel.DataAnnotations;

namespace FinanceManagerBackend.API.Models.Currencies;

/// <summary>
/// Create currency request.
/// </summary>
public class CreateCurrencyRequest : BaseCreateRequest
{
    /// <summary>
    /// Currency name.
    /// </summary>
    /// <example>RUB</example>
    [Required, MinLength(3), MaxLength(3)]
    public string Name { get; set; }
}