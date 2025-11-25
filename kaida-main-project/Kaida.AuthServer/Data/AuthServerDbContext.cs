using Kaida.AuthServer.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kaida.AuthServer.Data;

public class AuthServerDbContext(DbContextOptions<AuthServerDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<App> Apps { get; set; } = null!;
    public DbSet<AppAccess> AppAccess { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppAccess>()
            .HasKey(a => new
            {
                a.UserId, a.AppId
            });

    }

}

