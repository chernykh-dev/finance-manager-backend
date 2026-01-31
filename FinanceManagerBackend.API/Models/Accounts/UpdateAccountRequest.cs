using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FinanceManagerBackend.API.Models.Accounts;

/// <summary>
/// Update account request model.
/// </summary>
public class UpdateAccountRequest : BaseUpdateRequest
{
    /// <summary>
    /// Account owner id.
    /// </summary>
    [JsonIgnore]
    public Guid UserId { get; set; }

    /// <summary>
    /// Updated account name.
    /// </summary>
    [Required]
    public string Name { get; set; }

    /// <summary>
    /// Updated account balance.
    /// </summary>
    [Required]
    public decimal Balance { get; set; }

    /// <summary>
    /// Updated account category id.
    /// </summary>
    [Required]
    public Guid CurrencyId { get; set; }
}