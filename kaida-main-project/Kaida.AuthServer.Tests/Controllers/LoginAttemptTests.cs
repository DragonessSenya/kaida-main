using Kaida.AuthServer.Data;
using Kaida.AuthServer.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Net;

public class LoginAttemptTests
{
    private AuthServerDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AuthServerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AuthServerDbContext(options);
    }

    private IHttpContextAccessor CreateContextWithIp(string ip)
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse(ip);

        var accessor = new Mock<IHttpContextAccessor>();
        accessor.Setup(a => a.HttpContext).Returns(context);
        return accessor.Object;
    }

    [Fact]
    public void First_failed_attempt_creates_record()
    {
        // Arrange
        var db = CreateDb();
        var accessor = CreateContextWithIp("1.1.1.1");
        var service = new LoginService(accessor, db);

        // Act
        var result = service.handleFailedLoginAttempts();

        // Assert
        Assert.False(result); // lockout should NOT happen yet
        Assert.Single(db.FailedLoginAttempts);
        Assert.Equal(1, db.FailedLoginAttempts.First().LoginAttempts);
    }

    [Fact]
    public void Lockout_triggers_after_5_attempts()
    {
        // Arrange
        var db = CreateDb();
        var accessor = CreateContextWithIp("5.5.5.5");
        var service = new LoginService(accessor, db);

        // Simulate 4 failed attempts
        for (int i = 0; i < 5; i++)
            service.handleFailedLoginAttempts();

        // Act — 5th attempt
        var isLocked = service.handleFailedLoginAttempts();

        // Assert
        Assert.True(isLocked);
        Assert.Equal(5, db.FailedLoginAttempts.First().LoginAttempts);
    }

    [Fact]
    public void Different_ips_do_not_share_attempts()
    {
        // Arrange
        var db = CreateDb();

        var serviceA = new LoginService(CreateContextWithIp("1.1.1.1"), db);
        var serviceB = new LoginService(CreateContextWithIp("2.2.2.2"), db);

        // Act
        serviceA.handleFailedLoginAttempts(); // IP 1
        serviceB.handleFailedLoginAttempts(); // IP 2

        // Assert
        Assert.Equal(2, db.FailedLoginAttempts.Count());
        Assert.Equal(1, db.FailedLoginAttempts.First(f => f.IpAddress == "1.1.1.1").LoginAttempts);
        Assert.Equal(1, db.FailedLoginAttempts.First(f => f.IpAddress == "2.2.2.2").LoginAttempts);
    }
}
