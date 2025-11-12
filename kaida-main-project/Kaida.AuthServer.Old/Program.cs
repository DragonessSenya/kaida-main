using Kaida.AuthServer.Config;
using Kaida.AuthServer.Data;
using Microsoft.AspNetCore.Identity;
using Kaida.AuthServer.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Duende.IdentityServer.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure DbContext
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("AuthDb")));

// Configure Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddDefaultTokenProviders();

// Configure IdentityServer
builder.Services.AddIdentityServer()
    .AddAspNetIdentity<IdentityUser>()
    .AddInMemoryApiScopes(Config.ApiScopes)
    .AddInMemoryClients(Config.Clients)
    .AddInMemoryIdentityResources(Config.IdentityResources)
    .AddDeveloperSigningCredential();

// Register the custom profile service that supplies requested profile claims (non-null)
builder.Services.AddScoped<IProfileService, AspNetIdentityProfileService>();

// Add JWT authentication (for client apps)
builder.Services.AddAuthentication()
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = "https://localhost:5001";
        options.TokenValidationParameters.ValidateAudience = false;
    });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Kaida AuthServer API", Version = "v1" });
});

var app = builder.Build();

// Ensure database schema exists / migrations applied before seeding or handling requests.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    // Apply migrations (creates tables) — safe for tests and development.
    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    await DbSeeder.SeedAsync(app.Services);
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Kaida AuthServer API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseIdentityServer();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();

public partial class Program { }