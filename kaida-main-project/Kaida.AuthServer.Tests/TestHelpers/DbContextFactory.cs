using Kaida.AuthServer.Data;
using Kaida.AuthServer.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;

namespace Kaida.AuthServer.Tests.TestHelpers
{
    public static class DbContextFactory
    {
        public static AuthServerDbContext CreateInMemory()
        {
            var options = new DbContextOptionsBuilder<AuthServerDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new AuthServerDbContext(options);
            var passwordHasher = new PasswordHasher<User>();

            // Seed an application
            var app = new App
            {
                AppId = Guid.NewGuid(),
                AppName = "DemoApp"
            };
            context.Apps.Add(app);

            // Seed a user
            var testUser = new User
            {
                UserId = Guid.NewGuid(),
                UserName = "testuser",
                Password = passwordHasher.HashPassword(null, "testPassword!")
            };
            context.Users.Add(testUser);

            // Seed a user access (join table)
            var userAccess = new AppAccess
            {
                UserId = testUser.UserId,
                AppId = app.AppId,
                User = testUser,
                App = app,
            };
            context.AppAccess.Add(userAccess);

            context.SaveChanges();

            return context;
        }
    }
}