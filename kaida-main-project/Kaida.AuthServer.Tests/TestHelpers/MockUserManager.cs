using Microsoft.AspNetCore.Identity;
using Moq;
using System.Collections.Generic;

namespace Kaida.AuthServer.Tests.TestHelpers
{
    public static class MockUserManager
    {
        public static Mock<UserManager<IdentityUser>> Create()
        {
            var store = new Mock<IUserStore<IdentityUser>>();
            var mgr = new Mock<UserManager<IdentityUser>>(
                store.Object, null, null, null, null, null, null, null, null);

            // Setup FindByNameAsync
            mgr.Setup(x => x.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync((string username) =>
                {
                    if (username == "testuser")
                        return new IdentityUser("testuser") { Id = "testuser-id", Email = "testuser@example.com" };
                    return null;
                });

            // Setup CheckPasswordAsync
            mgr.Setup(x => x.CheckPasswordAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
                .ReturnsAsync((IdentityUser user, string password) =>
                    user.UserName == "testuser" && password == "Test@1234"
                );

            return mgr;
        }
    }
}