using Kaida.AuthServer.Data;
using Kaida.AuthServer.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;



var builder = WebApplication.CreateBuilder(args);

var dbConnectionString = builder.Configuration["DataSource"];
var appKey = builder.Configuration["JwtSettings:AuthServer:Key"];
var issuer = builder.Configuration["JwtSettings:AuthServer:Issuer"];
if (string.IsNullOrEmpty(appKey) || string.IsNullOrEmpty(issuer))
    throw new InvalidOperationException("JWT Key or Issuer is missing in configuration");

builder.Services.AddDbContext<AuthServerDbContext>(options =>
    options.UseSqlite(dbConnectionString));

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appKey))
        };
    });

builder.Services.AddScoped<JwtTokenService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
