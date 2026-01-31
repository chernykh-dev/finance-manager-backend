namespace FinanceManagerBackend.API.Models;

/// <summary>
/// Base create request.
/// </summary>
public class BaseCreateRequest
{
    /// <summary>
    /// Try to create entity with id (if set).
    /// </summary>
    public Guid? Id { get; set; }
}