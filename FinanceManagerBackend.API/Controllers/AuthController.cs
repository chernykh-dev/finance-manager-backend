using FinanceManagerBackend.API.Configuration;
using FinanceManagerBackend.API.Domain;
using FinanceManagerBackend.API.Domain.Entities;
using FinanceManagerBackend.API.HttpPipelines;
using FinanceManagerBackend.API.Models.Users;
using FinanceManagerBackend.API.Options;
using FinanceManagerBackend.API.Services;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FinanceManagerBackend.API.Controllers;

public class AuthController(IEntityRepository<User> userRepository, IAuthService authService) : BaseController
{
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> CreateUserAsync([FromBody] UserCredentials credentials,
        CancellationToken cancellationToken = default)
    {
        var entity = credentials.Adapt<User>();

        var entityWithUsername = await userRepository.GetByReadonlyAsync(x => x.Name == credentials.Name, cancellationToken);
        if (entityWithUsername != null)
        {
            return Conflict();
        }

        entity.Id = Guid.NewGuid();
        entity.Password = AuthHelper.EncryptPassword(credentials.Password);
        var result = GenerateResultForUser(entity);

        await userRepository.CreateAsync(entity, cancellationToken);

        return CreatedAtAction("Login", result);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> LoginAsync([FromBody] UserCredentials credentials,
        CancellationToken cancellationToken = default)
    {
        var entity =
            await userRepository.GetByAsync(
                x => x.Name == credentials.Name && x.Password == AuthHelper.EncryptPassword(credentials.Password),
                cancellationToken);

        if (entity == null)
        {
            return BadRequest("Username or password is incorrect");
        }

        var result = GenerateResultForUser(entity);

        await userRepository.UpdateAsync(entity, cancellationToken);

        return Ok(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> LogoutAsync(CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();

        var entity = await userRepository.GetByAsync(x => x.Id == userId, cancellationToken);

        if (entity == null)
        {
            return NotFound("User not found");
        }

        entity.ExpiredAt = DateTimeOffset.UtcNow;

        await userRepository.UpdateAsync(entity, cancellationToken);

        return Ok();
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshAsync([FromBody] RefreshRequest request, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();

        var entity = await userRepository.GetByAsync(
            x => x.Id == userId && x.RefreshToken == request.RefreshToken, cancellationToken);

        if (entity == null)
        {
            return NotFound("User not found");
        }

        var result = GenerateResultForUser(entity);

        await userRepository.UpdateAsync(entity, cancellationToken);

        return Ok(result);
    }

    private AuthResponse GenerateResultForUser(User entity)
    {
        var result = new AuthResponse()
        {
            AccessToken = authService.GetAccessToken(entity, out var expiresAt),
            RefreshToken = authService.GetRefreshToken()
        };

        entity.ExpiredAt = expiresAt;
        entity.RefreshToken = result.RefreshToken;

        return result;
    }
}