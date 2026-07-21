using System.Buffers.Text;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FinanceManagerBackend.API.Configuration;
using FinanceManagerBackend.API.Domain;
using FinanceManagerBackend.API.Domain.Entities;
using FinanceManagerBackend.API.HttpPipelines;
using FinanceManagerBackend.API.Models.Auth.OAuth.Telegram;
using FinanceManagerBackend.API.Models.Auth.OAuth.Yandex;
using FinanceManagerBackend.API.Models.Users;
using FinanceManagerBackend.API.Options;
using FinanceManagerBackend.API.Services;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FinanceManagerBackend.API.Controllers;

/// <summary>
/// Auth controller.
/// </summary>
/// <param name="userRepository"></param>
/// <param name="authService"></param>
public class AuthController(IEntityRepository<User> userRepository, IAuthService authService, IHttpClientFactory httpClientFactory) : BaseController
{
    /// <summary>
    /// Register new user.
    /// </summary>
    /// <param name="credentials"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Login exist user.
    /// </summary>
    /// <param name="credentials"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> LoginAsync([FromBody] UserCredentials credentials,
        CancellationToken cancellationToken = default)
    {
        var entity =
            await userRepository.GetByAsync(
                x => x.Name == credentials.Name,
                cancellationToken);

        if (!AuthHelper.VerifyPassword(credentials.Password, entity?.Password ?? ""))
        {
            entity = null;
        }

        if (entity == null)
        {
            return BadRequest("Username or password is incorrect");
        }

        var result = GenerateResultForUser(entity);

        await userRepository.UpdateAsync(entity, cancellationToken);

        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("oauth/yandex")]
    public async Task<ActionResult<AuthResponse>> YandexOAuthAsync([FromBody] YandexOAuthRequest request,
        CancellationToken cancellationToken = default)
    {
        var httpClient = httpClientFactory.CreateClient();

        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("OAuth", request.AccessToken);

        var response = await httpClient.GetAsync("https://login.yandex.ru/info", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return Unauthorized();
        }

        var yandexUserInfo = await response.Content.ReadFromJsonAsync<YandexUserInfo>(new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            },
            cancellationToken);

        if (yandexUserInfo == null)
        {
            // todo log.

            return Unauthorized();
        }

        var user = await userRepository.GetByAsync(x => x.YandexId == yandexUserInfo.Id, cancellationToken);

        var userExists = user != null;
        user ??= new User()
        {
            Id = Guid.NewGuid(),
            YandexId = yandexUserInfo.Id,
            Name = yandexUserInfo.Login
        };

        var result = GenerateResultForUser(user);

        if (userExists)
        {
            await userRepository.UpdateAsync(user, cancellationToken);
        }
        else
        {
            await userRepository.CreateAsync(user, cancellationToken);
        }

        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("oauth/telegram")]
    public async Task<ActionResult<AuthResponse>> TelegramOAuthAsync([FromBody] TelegramOAuthRequest request,
        CancellationToken cancellationToken = default)
    {
        var token = "C-tosjdHH0u2KncYZc7uWyObVr0&state=sdfsgdf";

        throw new NotImplementedException();
    }

    [AllowAnonymous]
    [HttpPost("oauth/vk/test")]
    public async Task<IActionResult> VkOAuthTestAsync(CancellationToken cancellationToken)
    {
        var url =
            "https://id.vk.ru/authorize?response_type=code&client_id=54661363&scope=email&redirect_uri=http://localhost/vkoauth&state=XXXRandomZZZ&code_challenge=Ptoe139wP7uS_UvXMYxyz4mwmbd4MeoGwU99JkNktDI&code_challenge_method=S256";

        var httpClient = httpClientFactory.CreateClient();

        return Redirect(url);
    }

    /// <summary>
    /// Logout current user.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("logout")]
    public async Task<IActionResult> LogoutAsync(CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();

        var entity = await userRepository.GetByAsync(x => x.Id == userId, cancellationToken);

        if (entity == null)
        {
            return NotFound("User not found");
        }

        entity.RefreshTokenExpiredAt = DateTimeOffset.UtcNow;

        await userRepository.UpdateAsync(entity, cancellationToken);

        return Ok();
    }

    /// <summary>
    /// Refresh current user.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> RefreshAsync([FromBody] RefreshRequest request, CancellationToken cancellationToken = default)
    {
        var userWithRequestToken = await userRepository
            .GetByReadonlyAsync(x => x.RefreshToken == request.RefreshToken, cancellationToken);

        if (userWithRequestToken == null)
        {
            return BadRequest("Invalid refresh token");
        }

        if (userWithRequestToken.RefreshTokenExpiredAt < DateTimeOffset.UtcNow)
        {
            return Unauthorized("Refresh token expired");
        }

        var result = GenerateResultForUser(userWithRequestToken);

        await userRepository.UpdateAsync(userWithRequestToken, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Delete current user.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete]
    public async Task<IActionResult> DeleteAsync(CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();

        var entity = await userRepository.GetByIdAsync(userId, cancellationToken);

        if (entity == null)
        {
            return NotFound("User not found");
        }

        await userRepository.DeleteAsync(entity, cancellationToken);

        return Ok();
    }

    private AuthResponse GenerateResultForUser(User entity)
    {
        var result = new AuthResponse()
        {
            AccessToken = authService.GetAccessToken(entity, out var _, out var refreshTokenExpiresAt),
            RefreshToken = authService.GetRefreshToken()
        };

        entity.RefreshTokenExpiredAt = refreshTokenExpiresAt;
        entity.RefreshToken = result.RefreshToken;

        return result;
    }
}