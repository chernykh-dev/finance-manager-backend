namespace FinanceManagerBackend.API.HttpPipelines;

public class UserRequestContext : IUserRequestContext
{
    public Guid UserId { get; set; }
}