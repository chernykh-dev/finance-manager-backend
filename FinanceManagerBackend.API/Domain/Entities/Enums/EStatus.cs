namespace FinanceManagerBackend.API.Domain.Entities.Enums;

/// <summary>
/// Entity status.
/// </summary>
public enum EStatus
{
    /// <summary>
    /// Active for show and operations.
    /// </summary>
    Active = 0,

    /// <summary>
    /// Hidden for show and active to operations.
    /// </summary>
    Hidden = 1,

    /// <summary>
    /// Hidden for show and operations.
    /// </summary>
    Deleted = 2
}