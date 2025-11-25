using Kaida.AuthServer.Entities;
using Kaida.AuthServer.Services;
using Kaida.AuthServer.Tests.TestHelpers;
using Microsoft.AspNetCore.Identity;
using Xunit.Abstractions;

namespace Kaida.AuthServer.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly UserService _userService;
        private readonly ITestOutputHelper _output;

        public AuthControllerTests(ITestOutputHelper output)
        {
            var dbContext = DbContextFactory.CreateInMemory();
            _output = output;

            _userService = new UserService(dbContext);
           return;

            [Fact]
             async Task ValidateUser_ReturnsUser_WhenCredentialsAreCorrect()
            {
                var db = DbContextFactory.CreateInMemory(); // fully isolated
                var userService = new UserService(db);

                var user = await userService.ValidateUserAsync("testuser", "testPassword!");

                Assert.False(user == null, "No User Was Found");
                _output?.WriteLine($"User was successfully obtained from the database.");
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
                Assert.True(!appsForUser.Any(), "AllowedApp List is Empty");
                
            }

            [Fact]
            async Task GetAllowedAppsForUserAsync_UserHasAccessToAnyApps()
            {
                var user = await _userService.ValidateUserAsync("testuser", "hashedpassword");
                if (user != null)
                {
                    var allowedAppsForUser = await _userService.GetAllowedAppsForUserAsync(user.UserId);

                    var appsForUser = allowedAppsForUser.ToList();
                    _output?.WriteLine($"Allowed apps count: {appsForUser?.Count()}");
                    Assert.True(appsForUser.Any(), "User has allowed Apps");
                }
            }
        }

    }
}
