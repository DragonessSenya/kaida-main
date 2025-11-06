using Kaida.AuthServer.Controllers;
using Kaida.AuthServer.Data;
using Kaida.AuthServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using Kaida.AuthServer.Tests.TestHelpers;
using System;
using System.Threading.Tasks;

namespace Kaida.AuthServer.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
        private readonly AuthDbContext _dbContext;
        private readonly Mock<IConfiguration> _configurationMock;

        public AuthControllerTests()
        {
            _userManagerMock = MockUserManager.Create();
            _dbContext = DbContextFactory.CreateInMemory();
            _configurationMock = new Mock<IConfiguration>();
            _configurationMock.Setup(c => c["Jwt:Secret"]).Returns("THIS_IS_A_LONG_TEST_SECRET_KEY_1234567890!");
        }

        [Fact]
        public async Task Login_ValidUser_ReturnsJwt()
        {
            var controller = new AuthController(_userManagerMock.Object, _dbContext, _configurationMock.Object);

            var request = new LoginRequest
            {
                Username = "testuser",
                Password = "Test@1234",
                AppId = _dbContext.Apps.First().Id
            };

            var result = await controller.Login(request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<LoginResponse>(okResult.Value);
            Assert.False(string.IsNullOrWhiteSpace(response.Token));
        }

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            var controller = new AuthController(_userManagerMock.Object, _dbContext, _configurationMock.Object);

            var request = new LoginRequest
            {
                Username = "testuser",
                Password = "WrongPassword",
                AppId = _dbContext.Apps.First().Id
            };

            var result = await controller.Login(request);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task Login_UserWithoutAccess_ReturnsForbid()
        {
            var controller = new AuthController(_userManagerMock.Object, _dbContext, _configurationMock.Object);

            // Use a random appId user does not have access to
            var request = new LoginRequest
            {
                Username = "testuser",
                Password = "Test@1234",
                AppId = Guid.NewGuid()
            };

            var result = await controller.Login(request);

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task Login_NonExistentUser_ReturnsUnauthorized()
        {
            var controller = new AuthController(_userManagerMock.Object, _dbContext, _configurationMock.Object);

            var request = new LoginRequest
            {
                Username = "nouser",
                Password = "password",
                AppId = _dbContext.Apps.First().Id
            };

            var result = await controller.Login(request);

            Assert.IsType<ForbidResult>(result);
        }
    }
}
