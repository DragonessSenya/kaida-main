using Kaida.AuthServer.Data;
using Kaida.AuthServer.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Kaida.AuthServer.Services;

public class UserService(AuthServerDbContext db)
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

    public async Task<RefreshToken> GenerateRefreshTokenForUserAsync(Guid userId, int daysValid = 7)
    {
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

    public bool ValidateRefreshTokenForUserAsync(Guid userId, string refreshToken)
    {
        var isUserValid =  db.RefreshTokens.Any(t => t.Token == refreshToken && t.UserId == userId && t.Expiration >= DateTime.UtcNow && t.IsRevoked == false);
        return isUserValid;
    }

    public async Task RevokeRefreshTokenForUser(RefreshToken refreshToken)
    {
        var oldToken =
             await db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == refreshToken.Token && t.UserId == refreshToken.UserId);
        if(oldToken != null)
        {
            oldToken.IsRevoked = true;
            await db.SaveChangesAsync();
        }
    }
}

