using System.ComponentModel.DataAnnotations;

namespace FinanceManagerBackend.API.Models.Transactions;

/// <summary>
/// Create transaction request model.
/// </summary>
public class CreateTransactionRequest : BaseCreateRequest
{
    /// <summary>
    /// Transaction account id.
    /// </summary>
    [Required]
    public Guid AccountId { get; set; }

    /// <summary>
    /// Transaction category id.
    /// </summary>
    [Required]
    public Guid CategoryId { get; set; }

    /// <summary>
    /// Transaction amount.
    /// </summary>
    [Required]
    public decimal Amount { get; set; }

    /// <summary>
    /// Transaction datetime.
    /// </summary>
    [Required]
    public DateTimeOffset DateTime { get; set; }

    /// <summary>
    /// Transaction comment.
    /// </summary>
    public string? Comment { get; set; }
}