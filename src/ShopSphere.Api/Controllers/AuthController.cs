using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ShopSphere.Api.DTOs;
using ShopSphere.Application.Authentication;

namespace ShopSphere.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserStore _userStore;
    private readonly ITokenService _tokenService;
    private readonly AuthOptions _options;

    public AuthController(IUserStore userStore, ITokenService tokenService, IOptions<AuthOptions> options)
    {
        _userStore = userStore;
        _tokenService = tokenService;
        _options = options.Value;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        var user = await _userStore.FindByUsernameAsync(dto.Username);
        if (user == null || !string.Equals(dto.Password, user.Password))
        {
            return Unauthorized(new { message = "Invalid credentials" });
        }

        var accessToken = _tokenService.GenerateToken(user);
        var expiresAt = DateTime.UtcNow.AddMinutes(_options.AccessTokenMinutes);

        return Ok(new LoginResponseDto(accessToken, user.Role, expiresAt));
    }
}
