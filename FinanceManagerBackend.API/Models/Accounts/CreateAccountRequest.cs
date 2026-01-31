using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FinanceManagerBackend.API.Models.Accounts;

/// <summary>
/// Create account request model.
/// </summary>
public class CreateAccountRequest : BaseCreateRequest
{
    /// <summary>
    /// Account owner id.
    /// </summary>
    [JsonIgnore]
    public Guid UserId { get; set; }

    /// <summary>
    /// Account name.
    /// </summary>
    [Required]
    public string Name { get; set; }

    /// <summary>
    /// Account balance (default is 0).
    /// </summary>
    [DefaultValue(0)]
    public decimal Balance { get; set; } = 0;

    /// <summary>
    /// Account currency id.
    /// </summary>
    [Required]
    public Guid CurrencyId { get; set; }
}