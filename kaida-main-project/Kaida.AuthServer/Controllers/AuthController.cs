using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using Duende.IdentityServer.Validation;
using Kaida.AuthServer.Data;
using Kaida.AuthServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    IConfiguration configuration,
    ITokenService tokenService,
    ITokenCreationService tokenCreationService,
    IClientStore clientStore,
    IResourceStore resourceStore,
    ITokenRequestValidator tokenRequestValidator) // <- new
    : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager = userManager;
    private readonly AuthDbContext _dbContext = dbContext;
    private readonly IConfiguration _configuration = configuration;
    private readonly ITokenService _tokenService = tokenService;
    private readonly ITokenCreationService _tokenCreationService = tokenCreationService;
    private readonly IClientStore _clientStore = clientStore;
    private readonly IResourceStore _resourceStore = resourceStore;
    private readonly ITokenRequestValidator _tokenRequestValidator = tokenRequestValidator;

    /// <summary>
    /// Authenticates a user for a specific application and returns an IdentityServer-issued access token if successful.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var user = await _userManager.FindByNameAsync(request.Username);
        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            return Unauthorized("Invalid credentials");

        var hasAccess = await _dbContext.UserAccesses
            .AnyAsync(x => x.UserId == user.Id && x.AppId == request.AppId);
        if (!hasAccess) return Forbid("User does not have access to this application");

        var app = await _dbContext.Apps.FindAsync(request.AppId);
        if (app == null) return BadRequest("Invalid application");

        var apiScope = app.Name.Contains("trello", StringComparison.OrdinalIgnoreCase)
            ? "trello_api"
            : "dashboard_api";
        var scopes = apiScope; // single scope string for the ROPC request

        // Resolve client id as before
        var clientId = _configuration[$"AppClientMap:{request.AppId}"] ?? "DemoApp";
        var client = await _clientStore.FindClientByIdAsync(clientId)
            ?? throw new InvalidOperationException($"Client '{clientId}' not found in client store.");

        // Build token endpoint parameters for ROPC
        var parameters = new System.Collections.Specialized.NameValueCollection
        {
            { "grant_type", "password" },
            { "username", request.Username },
            { "password", request.Password },
            { "scope", scopes },
            { "client_id", client.ClientId } // optional if client already known
        };

        // Validate the token request using IdentityServer pipeline
        var tokenRequestValidationContext = new Duende.IdentityServer.Validation.TokenRequestValidationContext
        {
            RequestParameters = parameters,
            ClientValidationResult = new Duende.IdentityServer.Validation.ClientSecretValidationResult
            {
                Client = client
            }
        };
        var validationResult = await _tokenRequestValidator.ValidateRequestAsync(tokenRequestValidationContext);
        if (validationResult.IsError)
            return BadRequest(validationResult.ErrorDescription ?? "invalid_request");

        // Use the validated request to create the token correctly
        var validatedRequest = validationResult.ValidatedRequest;
        var subject = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim("appId", request.AppId.ToString()),
            new Claim("name", user.UserName ?? string.Empty)
        }, "password"));

        // Ensure the subject is attached (the validator may have set it from resource owner validation)
        validatedRequest.Subject = validatedRequest.Subject ?? subject;

        var creationRequest = new Duende.IdentityServer.Models.TokenCreationRequest
        {
            Subject = validatedRequest.Subject,
            ValidatedRequest = validatedRequest,
            ValidatedResources = validatedRequest.ValidatedResources
        };

        var accessToken = await _tokenService.CreateAccessTokenAsync(creationRequest);
        var jwt = await _tokenCreationService.CreateTokenAsync(accessToken);

        var response = new LoginResponse
        {
            Token = jwt,
            Expiration = DateTime.UtcNow.AddSeconds(accessToken.Lifetime)
        };

        return Ok(response);
    }
}









