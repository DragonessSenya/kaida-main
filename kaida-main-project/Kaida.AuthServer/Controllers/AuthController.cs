using Kaida.AuthServer.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;



namespace Kaida.AuthServer.Controllers;

public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly AuthDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public AuthController(
        UserManager<IdentityUser> userManager,
        AuthDbContext dbContext,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _configuration = configuration;

        if(!ModelState.IsValid)
            return BadRequest(ModelState);
    }

    
}



