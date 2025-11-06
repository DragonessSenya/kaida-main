using System;
using System.Linq;
using Duende.IdentityServer.Models;
using Kaida.AuthServer.Config;
using Kaida.AuthServer.Data;
using Kaida.AuthServer.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Kaida.AuthServer.Tests.Integration;

public class AuthIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthIntegrationTests(WebApplicationFactory<Program> factory)
    {
        // Use the real application's services and DB. Ensure the app runs in Development so seeder runs on startup.
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("environment", "Development");
        });
    }

    [Fact]
    public async Task Login_ReturnsToken_ForSeededTestUserAndApp()
    {
        using var client = _factory.CreateClient();

        // Ensure DB seeded: resolve scoped services to seed apps & testuser (DbSeeder is called on startup in Development)
        // Query the app's DB to get an AppId
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        // Wait for seeder or seed explicitly if needed
        if (!db.Apps.Any())
        {
            // simple fallback seed
            var app = new Kaida.AuthServer.Data.Application(Guid.NewGuid(), "DemoApp");
            db.Apps.Add(app);
            db.SaveChanges();
        }

        var appId = db.Apps.First().Id;

        var request = new LoginRequest
        {
            Username = "testuser",
            Password = "Password123!",
            AppId = appId
        };

        var resp = await client.PostAsJsonAsync("/api/auth/login", request);

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

        var body = await resp.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.False(string.IsNullOrWhiteSpace(body?.Token));

        var text = await resp.Content.ReadAsStringAsync();
        // use debugger or temporarily Assert to surface text:
        System.Diagnostics.Debug.WriteLine(text);
    }

  
}