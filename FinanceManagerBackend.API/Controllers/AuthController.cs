using FinanceManagerBackend.API.Configuration;
using FinanceManagerBackend.API.Domain;
using FinanceManagerBackend.API.Domain.Entities;
using FinanceManagerBackend.API.Models.Users;
using FinanceManagerBackend.API.Options;
using FinanceManagerBackend.API.Services;
using Mapster;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;

namespace FinanceManagerBackend.API.Controllers;

public class AuthController(IEntityRepository<User> userRepository, IAuthService authService, IOptions<AuthOptions> authOptions) : BaseController
{
    [HttpPost]
    public async Task<IActionResult> CreateUserAsync([FromBody] UserCredentials credentials,
        CancellationToken cancellationToken = default)
    {
        var entity = credentials.Adapt<User>();
        entity.Id = Guid.NewGuid();

        entity.Password = AuthHelper.EncryptPassword(credentials.Password);

        try
        {
            await userRepository.CreateAsync(entity, cancellationToken);
        }
        catch (DbUpdateException exception)
        {
            if (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                return Conflict();
            }
        }

        var result = new AuthResponse()
        {
            AccessToken = authService.GetAccessToken(entity, out var expiresAt),
            RefreshToken = authService.GetRefreshToken()
        };

        entity.ExpiredAt = expiresAt;
        entity.RefreshToken = result.RefreshToken;

        await userRepository.UpdateAsync(entity, cancellationToken);

        return CreatedAtAction("Login", result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody] UserCredentials credentials,
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

        var result = new AuthResponse()
        {
            AccessToken = authService.GetAccessToken(entity, out var expiresAt),
            RefreshToken = authService.GetRefreshToken()
        };

        entity.ExpiredAt = expiresAt;
        entity.RefreshToken = result.RefreshToken;

        await userRepository.UpdateAsync(entity, cancellationToken);

        return Ok(result);
    }

}