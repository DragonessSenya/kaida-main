using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Kaida.AuthServer.Data;
using Kaida.AuthServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite.Query.Internal;
using Microsoft.IdentityModel.Tokens;

namespace Kaida.AuthServer.Controllers;

/// <summary>
/// Controller responsible for authentication and authorization of users.
/// Handles login requests and issues JWT tokens for specific applications.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController(
    UserManager<IdentityUser> userManager,
    AuthDbContext dbContext,
    IConfiguration configuration)
    : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager = userManager;
    private readonly AuthDbContext _dbContext = dbContext;
    private readonly IConfiguration _configuration = configuration;

    /// <summary>
    /// Authenticates a user for a specific application and returns a JWT if successful.
    /// </summary>
    /// <param name="request">The login request containing username, password, and the appId to access.</param>
    /// <returns>
    /// 200 OK with <see cref="LoginResponse"/> containing JWT and expiration if successful.  
    /// 400 BadRequest if model validation fails.  
    /// 401 Unauthorized if credentials are invalid.  
    /// 403 Forbid if the user does not have access to the requested application.
    /// </returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userManager.FindByNameAsync(request.Username);
        if (user != null && !await _userManager.CheckPasswordAsync(user, request.Password))
            return Unauthorized("Invalid Credentials");

        var hasAccess =
            await _dbContext.UserAccesses.AnyAsync(x =>
                user != null && x.UserId == user.Id && x.AppId == request.AppId);

        if (!hasAccess)
            return Forbid("User does not have access to this application");

        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]
        ?? throw new InvalidOperationException("JWT secret is not configured."));
        
        var tokenHandler = new JwtSecurityTokenHandler
        {
            MaximumTokenSizeInBytes = 16 * 1024
        };

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user!.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new Claim("appId", request.AppId.ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)

        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        var response = new LoginResponse
        {
            Token = tokenHandler.WriteToken(token),
            Expiration = tokenDescriptor.Expires!.Value
        };

        return Ok(response);
    }
}



