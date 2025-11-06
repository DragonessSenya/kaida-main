using System.IO;
using Kaida.AuthServer.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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
            // reload to ensure Id populated
            testUser = await userManager.FindByNameAsync("testuser");
        }

        // 2. Discover projects in the solution and create Application records for each project
        var solutionRoot = FindSolutionDirectory() ?? Directory.GetCurrentDirectory();

        var projectFiles = Directory.EnumerateFiles(solutionRoot, "*.csproj", SearchOption.AllDirectories)
            // ignore build artifacts and obj folders
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                        && !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Normalize current project file name to exclude it from apps
        const string currentProjectFileName = "Kaida.AuthServer.csproj";

        foreach (var proj in projectFiles)
        {
            var projFileName = Path.GetFileName(proj);
            if (string.Equals(projFileName, currentProjectFileName, StringComparison.OrdinalIgnoreCase))
                continue;

            // Skip test projects (common convention)
            if (projFileName.EndsWith(".Tests.csproj", StringComparison.OrdinalIgnoreCase)
                || projFileName.EndsWith(".Test.csproj", StringComparison.OrdinalIgnoreCase))
                continue;

            var appName = Path.GetFileNameWithoutExtension(projFileName);
            if (string.IsNullOrWhiteSpace(appName))
                continue;

            // Create the Application record if it doesn't exist
            var existing = await context.Apps.FirstOrDefaultAsync(a => a.Name == appName);
            if (existing == null)
            {
                var app = new Application(Guid.NewGuid(), appName);
                context.Apps.Add(app);
                await context.SaveChangesAsync();
                existing = app;
            }

            // Give the test user Admin access to the app if not already present
            if (testUser != null && !await context.UserAccesses.AnyAsync(x => x.UserId == testUser.Id && x.AppId == existing.Id))
            {
                context.UserAccesses.Add(new UserAccess
                {
                    UserId = testUser.Id,
                    AppId = existing.Id,
                    AccessLevel = "Admin"
                });
                await context.SaveChangesAsync();
            }
        }

        // If no projects found (fallback), ensure there's at least one demo App (backwards compatible)
        if (!context.Apps.Any())
        {
            var demo = new Application(Guid.NewGuid(), "DemoApp");
            context.Apps.Add(demo);
            await context.SaveChangesAsync();

            if (testUser != null && !context.UserAccesses.Any())
            {
                context.UserAccesses.Add(new UserAccess
                {
                    UserId = testUser.Id,
                    AppId = demo.Id,
                    AccessLevel = "Admin"
                });
                await context.SaveChangesAsync();
            }
        }
    }

    // Walks up from the executing directory to find a directory containing a .sln file.
    private static string? FindSolutionDirectory()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            try
            {
                if (dir.GetFiles("*.sln", SearchOption.TopDirectoryOnly).Any())
                    return dir.FullName;
            }
            catch
            {
                // ignore access issues and continue walking up
            }
            dir = dir.Parent;
        }

        // fallback: try current working directory
        dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir != null)
        {
            try
            {
                if (dir.GetFiles("*.sln", SearchOption.TopDirectoryOnly).Any())
                    return dir.FullName;
            }
            catch
            {
            }
            dir = dir.Parent;
        }

        return null;
    }
}