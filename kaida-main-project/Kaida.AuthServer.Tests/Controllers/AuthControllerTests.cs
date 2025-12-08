using Kaida.AuthServer.Entities;
using Kaida.AuthServer.Services;
using Kaida.AuthServer.Tests.TestHelpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Xunit.Abstractions;

namespace Kaida.AuthServer.Tests.Controllers;

public class AuthControllerTests
{
    private readonly UserService _userService;
    private readonly ITestOutputHelper _output;

    public AuthControllerTests(ITestOutputHelper output)
    {
        var dbContext = DbContextFactory.CreateInMemory();
        _output = output;
        IConfiguration config = new ConfigurationBuilder().Build();
        _userService = new UserService(dbContext, config);
    }


    [Fact]
    async Task ValidateUserAsync_WhenCredentialsAreNotCorrect()
    {
        var user = await _userService.ValidateUserAsync("nouser", "password");

        Assert.Null(user);
    }

    [Fact]
    async Task GetAllowedAppsForUserAsync_UserHasNoAccessToAnyApps()
    {
        var allowedAppsForUser = await _userService.GetAllowedAppsForUserAsync(Guid.NewGuid());

        var appsForUser = allowedAppsForUser.ToList();
        _output?.WriteLine($"Allowed apps count: {appsForUser?.Count()}");
        Assert.True(appsForUser != null && !appsForUser.Any(), "AllowedApp List is Empty");
    }

    [Fact]
    async Task GetAllowedAppsForUserAsync_UserHasAccessToAnyApps()
    {
        var user = await _userService.ValidateUserAsync("testuser", "hashedpassword");
        if (user != null)
        {
            var allowedAppsForUser = await _userService.GetAllowedAppsForUserAsync(user.UserId);

            var appsForUser = allowedAppsForUser.ToList();
            _output?.WriteLine($"Allowed apps count: {appsForUser?.Count}");
            Assert.True(appsForUser != null && appsForUser.Count != 0, "User has allowed Apps");
        }
    }

    [Fact]
    async Task TestBruteForceProtection()
    {

    }
    }


