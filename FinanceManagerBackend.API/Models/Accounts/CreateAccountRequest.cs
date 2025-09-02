using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FinanceManagerBackend.API.Models.Accounts;

public class CreateAccountRequest : BaseCreateRequest
{
    [JsonIgnore]
    public Guid UserId { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public decimal Balance { get; set; }

    [Required]
    public Guid CurrencyId { get; set; }
}