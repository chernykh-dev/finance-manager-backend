using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace FinanceManagerBackend.API.Domain.Entities;

/// <summary>
/// Currency entity.
/// </summary>
public class Currency : BaseEntity
{
    /// <summary>
    /// Currency name.
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// Currency symbol.
    /// </summary>
    public string? Symbol { get; set; }
}