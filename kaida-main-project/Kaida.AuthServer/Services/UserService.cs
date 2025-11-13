using Kaida.AuthServer.Data;
using Kaida.AuthServer.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace Kaida.AuthServer.Services;

public class UserService(AuthServerDbContext db)
{

    public async Task<User?> ValidateUserAsync(string username, string password)
    {
        var passwordHasher = new PasswordHasher<User>();

        var validatedUser = await db.Users
            .FirstOrDefaultAsync(u => u.UserName == username);
        if (validatedUser == null) return null;

        var verifiedResult = passwordHasher.VerifyHashedPassword(validatedUser, validatedUser.Password, password);
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
}

