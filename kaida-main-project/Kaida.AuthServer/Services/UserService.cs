using Kaida.AuthServer.Data;
using Kaida.AuthServer.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Security.Cryptography;

namespace Kaida.AuthServer.Services;

public class UserService(AuthServerDbContext db, IConfiguration config)
{
    private readonly PasswordHasher<User> _passwordHasher = new PasswordHasher<User>();
    public async Task<User?> ValidateUserAsync(string username, string password)
    {
        var validatedUser = await db.Users
            .FirstOrDefaultAsync(u => u.UserName == username);
        if (validatedUser == null) return null;

        var verifiedResult = _passwordHasher.VerifyHashedPassword(validatedUser, validatedUser.Password, password);
        if (verifiedResult == PasswordVerificationResult.Success)
        {
            return validatedUser;
        }
        return null;

    }

    public async Task<IEnumerable<AppAccess>> GetAllowedAppsForUserAsync(Guid userId)
    {
        var allowedAppsForUser = await db.AppAccess.Where(x => x.UserId == userId).ToListAsync();
        return allowedAppsForUser;


    }

    public async Task<RefreshToken> GenerateRefreshTokenForUserAsync(Guid userId)
    {
        var daysValid = GetRefreshTokenExpirationDays();
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        var refreshToken = new RefreshToken
        {
            UserId = userId,
            Token = token,
            Expiration = DateTime.UtcNow.AddDays(daysValid)
        };
        db.RefreshTokens.Add(refreshToken);
        await db.SaveChangesAsync();

        return refreshToken;
    }

    public async Task<RefreshToken?> ValidateAndGenerateRefreshTokenForUserAsync(Guid userId, string refreshToken)
    {
        var daysValid = GetRefreshTokenExpirationDays();
        var oldToken =
              await db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == refreshToken && t.UserId == userId && t.IsRevoked == false && t.Expiration >= DateTime.UtcNow);

        if (oldToken == null)
            return null;

        oldToken.IsRevoked = true;
        
        var newRefreshToken = new RefreshToken
        {
            UserId = userId,
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            Expiration = DateTime.UtcNow.AddDays(daysValid)
        };

        db.RefreshTokens.Add(newRefreshToken);
        await db.SaveChangesAsync();

        return refreshToken != null ? newRefreshToken : null;
    }


    public int GetRefreshTokenExpirationDays()
    {
        // Read from configuration
        return config.GetValue<int>("JwtSettings:AuthServer:RefreshTokenExpirationDays");
    }

}

