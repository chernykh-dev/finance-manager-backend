namespace FinanceManagerBackend.API.HttpPipelines;

public interface IUserRequestContext
{
    Guid UserId { get; set; }
}