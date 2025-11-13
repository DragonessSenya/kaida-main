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
public class AuthController(UserService userService) : ControllerBase
{
    private readonly JwtTokenService? _tokenService;

    [HttpPost("login")]
    public IActionResult Login(LoginRequest request)
    {
        // 1. Validate user credentials
        User user = userService.ValidateUser(request.Username, request.Password);
        //if (user == null) return SystemException();

        // 2. Determine allowed apps
        var allowedApps = GetAllowedAppsForUser(user.Id);

        // 3. Build claims model
        var claimsModel = new JwtClaimModel
        {
            UserId = user.Id,
            Apps = allowedApps
        };

        // 4. Generate JWT
        var token = _tokenService?.GenerateJwtToken(claimsModel);

        // 5. Return token (and refresh token if you implement one)
        return Ok(new { AccessToken = token });
    }

    private IEnumerable<string> GetAllowedAppsForUser(object id)
    {
        throw new NotImplementedException();
    }
}