using Kaida.AuthServer.Entities;
using Microsoft.AspNetCore.Identity;

namespace Kaida.AuthServer.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        // 1. Create test user if it doesn't exist
        var testUser = await userManager.FindByNameAsync("testuser");
        if (testUser == null)
        {
            testUser = new IdentityUser { UserName = "testuser", Email = "test@example.com" };
            await userManager.CreateAsync(testUser, "Password123!");
        }

        // 2. Create a test app if it doesn't exist
        if (!context.Apps.Any())
        {
            var app = new Application( Guid.NewGuid(),"TestApp" );
            context.Apps.Add(app);
            await context.SaveChangesAsync();

            // 3. Give the test user access to the app
            if (!context.UserAccesses.Any())
            {
                context.UserAccesses.Add(new UserAccess
                {
                    Id = 0,
                    UserId = testUser.Id,
                    AppId = app.Id,
                    AccessLevel = "Admin",
                });
                await context.SaveChangesAsync();
            }
        }
    }
}