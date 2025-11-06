using Kaida_Identity_Server.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add Services to the container.
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("AuthDb")));

var app = builder.Build();

//Middleware configuration.
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Endpoint configuration.
app.MapGet("/server-status", () => "AuthServer running!");
app.Run();
