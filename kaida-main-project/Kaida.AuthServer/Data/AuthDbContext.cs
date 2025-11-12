using Kaida.AuthServer.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Kaida.AuthServer.Data
{
    /// <summary>
    /// Database context for the authentication server.
    /// Inherits from IdentityDbContext to include ASP.NET Identity tables,
    /// and adds application-specific entities.
    /// </summary>
    public class AuthDbContext(DbContextOptions<AuthDbContext> options) : IdentityDbContext<IdentityUser>(options)
    {
        /// <summary>
        /// Gets or sets the collection of registered applications.
        /// </summary>
        public DbSet<Application> Apps { get; set; } = null!;


        /// <summary>
        /// Gets or sets the collection of user access records.
        /// </summary> 
        public DbSet<UserAccess> UserAccesses { get; set; } = null!;

        /// <summary>
        /// Configures the model relationships and constraints using Fluent API.
        /// </summary>
        /// <param name=modelBuilder>The model builder to configure the entity relationships.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //Application entity configuration
            modelBuilder.Entity<Application>(entity =>
            {
                entity.HasKey(e => e.AppId);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            });

            //UserAccess entity configuration
            modelBuilder.Entity<UserAccess>(entity => {
                entity.HasKey(e => e.AppId);
               
            });

            modelBuilder.Entity<UserAccess>()
                .HasOne<IdentityUser>()
                .WithMany()
                .HasForeignKey(ua => ua.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserAccess>()
                .HasOne(ua => ua.App)
                .WithMany(a => a.UserAccesses)
                .HasForeignKey(ua => ua.AppId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
