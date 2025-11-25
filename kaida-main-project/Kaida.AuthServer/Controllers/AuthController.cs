using Kaida.AuthServer.Entities;
using Kaida.AuthServer.Models;
using Kaida.AuthServer.Services;
using Microsoft.AspNetCore.Mvc;
using LoginRequest = Kaida.AuthServer.Models.LoginRequest;

namespace Kaida.AuthServer.Controllers;


[ApiController]
[Route("api/[controller]")]
public class AuthController(UserService userService, JwtTokenService tokenService) : ControllerBase
{
    private readonly JwtTokenService _tokenService = tokenService;

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync(LoginRequest request)
    {
        try
        {
            // 1. Validate user credentials
            var user = await userService.ValidateUserAsync(request.Username, request.Password);
            if (user == null) return Unauthorized(new { Message = "Your not authorized" });

            // 2. Determine allowed apps
            var allowedApps = (await userService.GetAllowedAppsForUserAsync(user.UserId)).ToList();
            if (!allowedApps.Any()) return Unauthorized(new { Message = "Your not authorized" });
            // 3. Build claims model
            var appsClaim = allowedApps.Select(a => a.AppId.ToString()).ToList();
            var claimsModel = new JwtClaimModel
            {
                UserId = user.UserId.ToString(),
                Apps = appsClaim
            };

            // 4. Generate JWT
            var token = _tokenService.GenerateJwtToken(claimsModel);
            var refreshToken = await userService.GenerateRefreshTokenForUserAsync(user.UserId, 7);

            // 5. Return token (and refresh token if you implement one)
            return Ok(new
            {
                AccessToken = token,
                RefreshToken = refreshToken.Token
            });
        }
        catch (Exception ex)
        {
            return Problem(ex.Message, statusCode: 500);
        }

    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var refreshTokenIsValid = userService.ValidateRefreshTokenForUserAsync(request.UserId, request.Token);
        if (!refreshTokenIsValid) return Unauthorized(new { Message = "Your not authorized" });

        var allowedApps = (await userService.GetAllowedAppsForUserAsync(request.UserId)).ToList();
        if (!allowedApps.Any()) return Unauthorized(new { Message = "Your not authorized" });

        // 3. Build claims model
        var appsClaim = allowedApps.Select(a => a.AppId.ToString()).ToList();
        var claimsModel = new JwtClaimModel
        {
            UserId = request.UserId.ToString(),
            Apps = appsClaim
        };

        // 4. Generate JWT
        var token = _tokenService.GenerateJwtToken(claimsModel);
        var refreshToken = await userService.GenerateRefreshTokenForUserAsync(request.UserId, 7);

        // 5. Return token (and refresh token if you implement one)
        return Ok(new
        {
            AccessToken = token,
            RefreshToken = refreshToken.Token
            });
    }

}