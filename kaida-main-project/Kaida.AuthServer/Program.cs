using Kaida.AuthServer.Config;
using Kaida.AuthServer.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

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
    .AddInMemoryApiScopes(Config.ApiScopes)     // You'll define scopes in Config.cs
    .AddInMemoryClients(Config.Clients)        // Define clients later
    .AddInMemoryIdentityResources(Config.IdentityResources)
    .AddDeveloperSigningCredential();          // Use proper signing cert in production

// Add JWT authentication (for client apps)
builder.Services.AddAuthentication()
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = "https://localhost:5001"; // your AuthServer URL
        options.TokenValidationParameters.ValidateAudience = false;
    });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Kaida AuthServer API", Version = "v1" });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await DbSeeder.SeedAsync(app.Services);
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Kaida AuthServer API v1");
        c.RoutePrefix = string.Empty; // Swagger at root URL
    });
}

app.UseHttpsRedirection();
app.UseIdentityServer();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();