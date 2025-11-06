using Duende.IdentityServer;
using Kaida_Identity_Server.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Kaida_Identity_Server.Extensions
{
    public static class IdentityServiceExtensions
    {
        public static IServiceCollection AddAuthServer(this IServiceCollection services, IConfiguration configuration)
        {
            // DbContext
            services.AddDbContext<AuthDbContext>(options =>
                options.UseSqlite(configuration.GetConnectionString("AuthDb")));

            // Identity
            services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
            })
            .AddEntityFrameworkStores<AuthDbContext>()
            .AddDefaultTokenProviders();

            // IdentityServer
            services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
            })
            .AddInMemoryIdentityResources(Config.IdentityResources)
            .AddInMemoryApiScopes(Config.ApiScopes)
            .AddInMemoryClients(Config.Clients)
            .AddAspNetIdentity<IdentityUser>();

            return services;
        }
    }
}
