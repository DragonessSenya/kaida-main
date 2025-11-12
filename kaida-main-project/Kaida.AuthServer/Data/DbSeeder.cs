using Kaida.AuthServer.Entities;
using Microsoft.AspNetCore.Identity;

namespace Kaida.AuthServer.Data;

public class DBSeeder
{
    internal static async Task SeedAsync(AuthDbContext dbContext, UserManager<IdentityUser> userManager)
    {
        if (!dbContext.Apps.Any())
        {
            var appsToSeed = new[] { "KaidaDashboard", "KaidaTaskboard" };

            foreach (var appName in appsToSeed)
            {
                var app = new Application
                {
                    Name = appName
                };
                dbContext.Apps.Add(app);
            }

            dbContext.SaveChanges();
        }

        var adminUsername = "Senya";
        var adminUser = await userManager.FindByNameAsync(adminUsername);

        if (adminUser == null)
        {
            adminUser = new IdentityUser
            {
                UserName = adminUsername,
                Email = ""
            };
            var result = await userManager.CreateAsync(adminUser, "Password123!");
            if (!result.Succeeded)
            {
                throw new Exception($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

        }


        foreach (var app in dbContext.Apps)
        {
            if (!dbContext.UserAccesses.Any(ua => ua.UserId == adminUser.Id && ua.AppId == app.AppId))
            {
                dbContext.UserAccesses.Add(new UserAccess
                {
                    UserId = adminUser.Id,
                    AppId = app.AppId,
                    AccessLevel = "Admin"
                });
            }
        }

        var dashboardTestUserName = "dashboard_tester";
        var testDashboardUser = await userManager.FindByNameAsync(dashboardTestUserName);

        if (testDashboardUser == null)
        {
            testDashboardUser = new IdentityUser
            {
                UserName = dashboardTestUserName,
                Email = ""
            };
            var result = await userManager.CreateAsync(testDashboardUser, "Password123!");
            if (!result.Succeeded)
            {
                throw new Exception($"Failed to create test dashboard user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
        var dashboardApp = dbContext.Apps.FirstOrDefault(a => a.Name == "KaidaDashboard");
        if (dashboardApp != null)
        {
            if (!dbContext.UserAccesses.Any(ua => ua.UserId == testDashboardUser.Id && ua.AppId == dashboardApp!.AppId))
            {
                dbContext.UserAccesses.Add(new UserAccess
                {
                    UserId = testDashboardUser.Id,
                    AppId = dashboardApp.AppId,
                    AccessLevel = "User"
                });
            }
        }
        await dbContext.SaveChangesAsync();

    }
}



