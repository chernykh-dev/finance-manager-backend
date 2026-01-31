namespace FinanceManagerBackend.API.Exceptions;

/// <summary>
/// Entity exists exception.
/// </summary>
public class EntityExistsException : Exception
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="entityType"></param>
    public EntityExistsException(Type entityType)
        : base($"Entity of type {entityType.Name} is already exists")
    {

    }
}