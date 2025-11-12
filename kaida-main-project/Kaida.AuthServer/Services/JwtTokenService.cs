using System.IdentityModel.Tokens.Jwt;
using Kaida.AuthServer.Models;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Kaida.AuthServer.Services
{
    public class JwtTokenService(IConfiguration configuration)
    {
        public string GenerateJwtToken(JwtClaimModel claims)
        {
            var appKey = configuration["JwtSettings:AuthServer:Key"];
            var issuer = configuration["JwtSettings:AuthServer:Issuer"];
            var accessTokenExpirationMinutes = configuration["JwtSettings:AuthServer:AccessTokenExpirationMinutes"];
            if (!double.TryParse(accessTokenExpirationMinutes, out var expirationMinutes))
                throw new InvalidOperationException("Invalid AccessTokenExpirationMinutes in config");

            if (string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(appKey) || string.IsNullOrEmpty(accessTokenExpirationMinutes))
                throw new InvalidOperationException("JWT Key or Issuer is missing in configuration");


            var jwtClaimList = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, claims.UserId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iss, issuer),
                new Claim("apps", string.Join(",", claims.Apps))
            };

            var keyBytes = Encoding.UTF8.GetBytes(appKey);
            var signedKey = new SymmetricSecurityKey(keyBytes);
            var creds = new SigningCredentials(signedKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                claims: jwtClaimList,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: creds
            );

            var tokenHandler = new JwtSecurityTokenHandler();


            return tokenHandler.WriteToken(token);

        }
    }
}
