using System.ComponentModel.DataAnnotations;

namespace FinanceManagerBackend.API.Models.Currencies;

public class CreateCurrencyRequest : BaseCreateRequest
{
    /// <summary>
    ///
    /// </summary>
    /// <example>RUB</example>
    [Required, MinLength(3), MaxLength(3)]
    public string Name { get; set; }
}