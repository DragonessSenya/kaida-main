//using Duende.IdentityServer.Models;
//using Duende.IdentityServer.Services;
//using Duende.IdentityServer.Stores;
//using Duende.IdentityServer.Validation;
//using Kaida.AuthServer.Data;
//using Kaida.AuthServer.Entities;
//using Kaida.AuthServer.Tests.TestHelpers;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Configuration;
//using Moq;

//namespace Kaida.AuthServer.Tests.Controllers
//{
//    public class AuthControllerTests
//    {
//        private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
//        private readonly AuthDbContext _dbContext;
//        private readonly Mock<IConfiguration> _configurationMock;
//        private readonly Mock<ITokenService> _tokenServiceMock;
//        private readonly Mock<ITokenCreationService> _tokenCreationServiceMock;
//        private readonly Mock<IClientStore> _clientStoreMock;
//        private readonly Mock<IResourceStore> _resourceStoreMock;
//        private readonly Mock<ITokenRequestValidator> _tokenRequestValidatorMock;

//        private readonly IdentityUser _validUser;
//        private Guid _validAppId;

//        public AuthControllerTests()
//        {
//            // --- Base setup for mocks ---
//            _userManagerMock = MockUserManager.Create();
//            _dbContext = DbContextFactory.CreateInMemory(Guid.NewGuid().ToString()); // isolated per test run

//            _configurationMock = new Mock<IConfiguration>();
//            _configurationMock.Setup(c => c["Jwt:Secret"])
//                .Returns("THIS_IS_A_LONG_TEST_SECRET_KEY_1234567890!");

//            _configurationMock.Setup(c => c[It.Is<string>(s => s.StartsWith("AppClientMap:"))])
//                .Returns("dashboard");

//            _tokenServiceMock = new Mock<ITokenService>();
//            _tokenCreationServiceMock = new Mock<ITokenCreationService>();
//            _clientStoreMock = new Mock<IClientStore>();
//            _resourceStoreMock = new Mock<IResourceStore>();
//            _tokenRequestValidatorMock = new Mock<ITokenRequestValidator>();

//            // --- IdentityServer and token mocks ---
//            _clientStoreMock.Setup(s => s.FindClientByIdAsync("dashboard"))
//                .ReturnsAsync(new Client
//                {
//                    ClientId = "dashboard",
//                    AllowedScopes = { "dashboard_api", "openid", "profile", "email" }
//                });

//            _tokenServiceMock.Setup(s => s.CreateAccessTokenAsync(It.IsAny<TokenCreationRequest>()))
//                .ReturnsAsync(new Token { Lifetime = 3600 });

//            _tokenCreationServiceMock.Setup(s => s.CreateTokenAsync(It.IsAny<Token>()))
//                .ReturnsAsync("dummy.jwt");

//            _tokenRequestValidatorMock
//                .Setup(v => v.ValidateRequestAsync(It.IsAny<TokenRequestValidationContext>()))
//                .ReturnsAsync(new TokenRequestValidationResult(new ValidatedTokenRequest
//                {
//                    Client = new Client { ClientId = "dashboard" },
//                    ValidatedResources = new ResourceValidationResult()
//                }));

//            // --- Seed a valid app and user ---
//            var app = new Application(Guid.NewGuid(), "dashboard_app");
//            _dbContext.Apps.Add(app);
//            _dbContext.SaveChanges();

//            _validUser = new IdentityUser
//            {
//                Id = "user1",
//                UserName = "testuser",
//                Email = "test@example.com"
//            };
//            _validAppId = app.Id;
//        }

//        // 🧩 Helper to reset DB between tests
//        private void ResetDatabase()
//        {
//            _dbContext.ChangeTracker.Clear();
//            _dbContext.Database.EnsureDeleted();
//            _dbContext.Database.EnsureCreated();

//            var app = new Application(Guid.NewGuid(), "dashboard_app");
//            _dbContext.Apps.Add(app);
//            _dbContext.SaveChanges();

//            _validAppId = app.Id;
//        }

//        // 🧩 Helper to configure user and access rights
//        private void SetupValidUser(bool withAccess = true)
//        {
//            _userManagerMock.Setup(u => u.FindByNameAsync("testuser"))
//                .ReturnsAsync(_validUser);

//            _userManagerMock.Setup(u => u.CheckPasswordAsync(_validUser, "Test@1234"))
//                .ReturnsAsync(true);

//            if (withAccess)
//            {
//                _dbContext.UserAccesses.Add(new UserAccess
//                {
//                    AppId = _validAppId,
//                    UserId = _validUser.Id,
//                    AccessLevel = "Admin"

//                });
//                _dbContext.SaveChanges();
//            }
//        }

//        // 🧪 Tests ------------------------------------------------------

//        [Fact]
//        public async Task Login_ValidUser_ReturnsJwt()
//        {
//            ResetDatabase();
//            SetupValidUser();

//            var controller = CreateController();

//            var request = new LoginRequest
//            {
//                Username = "testuser",
//                Password = "Test@1234",
//                AppId = _validAppId
//            };

//            var result = await controller.Login(request);

//            var okResult = Assert.IsType<OkObjectResult>(result);
//            var response = Assert.IsType<LoginResponse>(okResult.Value);
//            Assert.False(string.IsNullOrWhiteSpace(response.Token));
//            Assert.Equal("dummy.jwt", response.Token);
//        }

//        [Fact]
//        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
//        {
//            ResetDatabase();
//            SetupValidUser();

//            _userManagerMock.Setup(u => u.CheckPasswordAsync(It.IsAny<IdentityUser>(), "WrongPassword"))
//                .ReturnsAsync(false);

//            var controller = CreateController();

//            var request = new LoginRequest
//            {
//                Username = "testuser",
//                Password = "WrongPassword",
//                AppId = _validAppId
//            };

//            var result = await controller.Login(request);
//            Assert.IsType<UnauthorizedObjectResult>(result);
//        }

//        [Fact]
//        public async Task Login_UserWithoutAccess_ReturnsForbid()
//        {
//            ResetDatabase();
//            SetupValidUser(withAccess: false);

//            var controller = CreateController();

//            var request = new LoginRequest
//            {
//                Username = "testuser",
//                Password = "Test@1234",
//                AppId = _validAppId
//            };

//            var result = await controller.Login(request);
//            Assert.IsType<ForbidResult>(result);
//        }

//        [Fact]
//        public async Task Login_NonExistentUser_ReturnsUnauthorized()
//        {
//            ResetDatabase();

//            _userManagerMock.Setup(u => u.FindByNameAsync("nouser"))
//                .ReturnsAsync((IdentityUser?)null);

//            var controller = CreateController();

//            var request = new LoginRequest
//            {
//                Username = "nouser",
//                Password = "password",
//                AppId = _validAppId
//            };

//            var result = await controller.Login(request);
//            Assert.IsType<UnauthorizedObjectResult>(result);
//        }

//        // 🔧 Helper for consistent controller creation
//        private AuthController CreateController() =>
//            new AuthController(
//                _userManagerMock.Object,
//                _dbContext,
//                _configurationMock.Object,
//                _tokenServiceMock.Object,
//                _tokenCreationServiceMock.Object,
//                _clientStoreMock.Object,
//                _resourceStoreMock.Object,
//                _tokenRequestValidatorMock.Object);
//    }
//}
