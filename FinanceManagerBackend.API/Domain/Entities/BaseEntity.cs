using Mapster;

namespace FinanceManagerBackend.API.Domain.Entities;

/// <summary>
/// Base entity class.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Entity id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Entity created at.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Entity updated at.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}