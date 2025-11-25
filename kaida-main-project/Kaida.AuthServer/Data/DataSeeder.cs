using Kaida.AuthServer.Data;
using Kaida.AuthServer.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthServerDbContext>();
        var hasher = new PasswordHasher<User>();

        await db.Database.MigrateAsync();

        // --- Test User ---
        if (!db.Users.Any())
        {
            var user = new User
            {
                UserId = Guid.NewGuid(),
                UserName = "test",
                Password = ""

            };

            user.Password = hasher.HashPassword(user, "test123!");

            db.Users.Add(user);
        }
        if (!db.Apps.Any(a => a.AppId == Guid.Parse("11111111-1111-1111-1111-111111111111")))
        {
            db.Apps.Add(new App
            {
                AppId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                AppName = "TestApp"
            });
        }
        await db.SaveChangesAsync();
        // --- Test App (Client) ---
        if (!db.AppAccess.Any())
        {
            var firstUser = db.Users.FirstOrDefault();
            if (firstUser != null)
            {
                var app = new AppAccess
                {
                    AppId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    UserId = firstUser.UserId
                };

                db.AppAccess.Add(app);
            }
        }

        await db.SaveChangesAsync();
    }
}
