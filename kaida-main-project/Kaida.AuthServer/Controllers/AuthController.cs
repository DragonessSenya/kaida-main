using Kaida.AuthServer.Entities;
using Kaida.AuthServer.Helpers;
using Kaida.AuthServer.Models;
using Kaida.AuthServer.Services;
using Kaida.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace Kaida.AuthServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(UserService userService, JwtTokenService tokenService) : ControllerBase
{
    private readonly JwtTokenService _tokenService = tokenService;

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync(Shared.Models.LoginRequest request)
    {
        try
        {
            // 1. Validate user credentials
            var user = await userService.ValidateUserAsync(request.Username, request.Password);
            if (user == null) return Unauthorized(new { Message = "Your not authorized" });

            // 2. Determine allowed apps
            var allowedApps = (await userService.GetAllowedAppsForUserAsync(user.UserId)).ToList();
            if (allowedApps.Count == 0) return Unauthorized(new { Message = "Your not authorized" });
            // 3. Build claims model
           
            var claimsModel = JwtHelper.BuildClaims(user.UserId, allowedApps);

            // 4. Generate JWT
            var token = _tokenService.GenerateJwtToken(claimsModel);
            var refreshToken = await userService.GenerateRefreshTokenForUserAsync(user.UserId);

            // 5. Return token and refresh token
            return Ok(new LoginResponse
            {
                AccessToken = token.Token,
                RefreshToken = refreshToken.Token,
                Expiration = token.Expiration
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
        var refreshToken = await userService.ValidateAndGenerateRefreshTokenForUserAsync(request.UserId, request.Token);
        if (refreshToken == null) return Unauthorized(new { Message = "Your not authorized" });

        var allowedApps = (await userService.GetAllowedAppsForUserAsync(request.UserId)).ToList();
        if (allowedApps.Count == 0) return Unauthorized(new { Message = "Your not authorized" });

        // 3. Build claims model
        var claimsModel = JwtHelper.BuildClaims(request.UserId, allowedApps);

        // 4. Generate JWT
        var token = _tokenService.GenerateJwtToken(claimsModel);

        // 5. Return token and refresh token
        return Ok(new LoginResponse
        {
            AccessToken = token.Token,
            RefreshToken = refreshToken.Token,
            Expiration = token.Expiration
        });
    }

}