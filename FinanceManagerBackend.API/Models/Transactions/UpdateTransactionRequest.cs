using System.ComponentModel.DataAnnotations;

namespace FinanceManagerBackend.API.Models.Transactions;

/// <summary>
/// Update transaction request.
/// </summary>
public class UpdateTransactionRequest : BaseUpdateRequest
{
    /// <summary>
    /// Updated transaction account id.
    /// </summary>
    [Required]
    public Guid AccountId { get; set; }

    /// <summary>
    /// Updated transaction category id.
    /// </summary>
    [Required]
    public Guid CategoryId { get; set; }

    /// <summary>
    /// Updated transaction amount.
    /// </summary>
    [Required]
    public decimal Amount { get; set; }

    /// <summary>
    /// Updated transaction datetime.
    /// </summary>
    [Required]
    public DateTimeOffset DateTime { get; set; }

    /// <summary>
    /// Updated transaction comment.
    /// </summary>
    public string? Comment { get; set; }
}