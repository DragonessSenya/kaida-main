//using Kaida.AuthServer.Data;
//using Kaida.AuthServer.Entities;
//using Microsoft.EntityFrameworkCore;
//using System;
//using System.Linq;

//namespace Kaida.AuthServer.Tests.TestHelpers
//{
//    public static class DbContextFactory
//    {
//        public static AuthDbContext CreateInMemory(string v)
//        {
//            var options = new DbContextOptionsBuilder<AuthDbContext>()
//                .UseInMemoryDatabase(Guid.NewGuid().ToString())
//                .Options;

//            var context = new AuthDbContext(options);

//            // Seed an application
//            var app = new Application(Guid.NewGuid(), "DemoApp");
//            context.Apps.Add(app);

//            // Seed a user access for testuser
//            var testUser = new Microsoft.AspNetCore.Identity.IdentityUser("testuser") { Id = "testuser-id" };
//            var userAccess = new UserAccess(0, testUser.Id, app.Id, "Admin")
//            {
//                User = testUser,
//                App = app,
//                UserId = testUser.Id,
//                AppId = app.Id
//            };
//            context.UserAccesses.Add(userAccess);

//            context.SaveChanges();

//            return context;
//        }
//    }
//}