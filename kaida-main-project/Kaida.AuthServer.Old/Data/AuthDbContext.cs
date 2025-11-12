using Kaida.AuthServer.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Kaida.AuthServer.Data
{
    /// <summary>
    /// The EF Core database context for the AuthServer.
    /// Manages users, roles, applications, and per-app access.
    /// Inherits from <see cref="IdentityDbContext"/> for ASP.NET Core Identity integration.
    /// </summary>
    public class AuthDbContext(DbContextOptions<AuthDbContext> options)
        : IdentityDbContext(options)
    {
        /// <summary>
        /// Applications registered in the AuthServer.
        /// </summary>
        public DbSet<Application> Apps { get; set; }

        /// <summary>
        /// User access entries linking users to specific applications.
        /// </summary>
        public DbSet<UserAccess> UserAccesses { get; set; }

        /// <summary>
        /// Configure EF Core model relationships and constraints.
        /// </summary>
        /// <param name="builder">The <see cref="ModelBuilder"/> instance.</param>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure UserAccess
            builder.Entity<UserAccess>(entity =>
            {
                entity.Property(x => x.UserId).HasMaxLength(450).IsRequired();
                entity.Property(x => x.AccessLevel).HasMaxLength(50);

                entity.HasOne(x => x.User)
                      .WithMany()
                      .HasForeignKey(x => x.UserId)
                      .IsRequired();

                entity.HasOne(x => x.App)
                      .WithMany(x => x.UserAccesses)
                      .HasForeignKey(x => x.AppId)
                      .IsRequired();
            });

            // Configure Application
            builder.Entity<Application>(entity =>
            {
                entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            });
        }
    }
}
