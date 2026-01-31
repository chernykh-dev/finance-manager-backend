namespace FinanceManagerBackend.API.Exceptions;

/// <summary>
/// Entity not found exception.
/// </summary>
public class EntityNotFoundException : Exception
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="entityType"></param>
    public EntityNotFoundException(Type entityType)
        : base($"Entity of type {entityType.Name} not found")
    {

    }
}