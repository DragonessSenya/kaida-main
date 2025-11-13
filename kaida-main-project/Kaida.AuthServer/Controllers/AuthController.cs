using Kaida.AuthServer.Data;
using Kaida.AuthServer.Models;
using Kaida.AuthServer.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using LoginRequest = Kaida.AuthServer.Models.LoginRequest;

namespace Kaida.AuthServer.Controllers;


[ApiController]
[Route("api/[controller]")]
public class AuthController(UserService userService, JwtTokenService? tokenService) : ControllerBase
{
    private readonly JwtTokenService? _tokenService = tokenService;

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync(LoginRequest request)
    {
        // 1. Validate user credentials
        var user = await userService.ValidateUserAsync(request.Username, request.Password);
        if (user == null) return Unauthorized(new {Message = "Your not authorized"});

        // 2. Determine allowed apps
        var allowedApps = (await userService.GetAllowedAppsForUserAsync(user.UserId)).ToList();
        if (!allowedApps.Any()) return Unauthorized(new { Message = "Your not authorized for any Apps" });
        // 3. Build claims model
        var appsClaim = allowedApps.Select(a => a.AppId.ToString()).ToList();
        var claimsModel = new JwtClaimModel
        {
            UserId = user.UserId.ToString(),
            Apps = appsClaim
        };

        // 4. Generate JWT
        var token = _tokenService?.GenerateJwtToken(claimsModel);

        // 5. Return token (and refresh token if you implement one)
        return Ok(new { AccessToken = token });
    }

}