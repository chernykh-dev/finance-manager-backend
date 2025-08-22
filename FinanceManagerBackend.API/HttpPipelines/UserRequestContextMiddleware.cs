namespace FinanceManagerBackend.API.HttpPipelines;

public class UserRequestContextMiddleware
{
    private readonly RequestDelegate _next;

    public UserRequestContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, IUserRequestContext userRequestContext)
    {
        var userIdClaim = context.User.Claims.FirstOrDefault(x => x.Type == "sub");

        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            throw new UnauthorizedAccessException("You are not authorized to access this resource.");
        }

        userRequestContext.UserId = userId;

        await _next(context);
    }
}