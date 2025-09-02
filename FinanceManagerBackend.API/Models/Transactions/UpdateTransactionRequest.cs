using System.ComponentModel.DataAnnotations;

namespace FinanceManagerBackend.API.Models.Transactions;

public class UpdateTransactionRequest : BaseUpdateRequest
{
    [Required]
    public Guid AccountId { get; set; }

    [Required]
    public Guid CategoryId { get; set; }

    [Required]
    public decimal Amount { get; set; }

    [Required]
    public DateTimeOffset DateTime { get; set; }

    public string? Comment { get; set; }
}