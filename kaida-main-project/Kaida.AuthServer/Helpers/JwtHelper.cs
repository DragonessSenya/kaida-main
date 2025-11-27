using Kaida.AuthServer.Entities;
using Kaida.AuthServer.Models;
using System.Security.Claims;

namespace Kaida.AuthServer.Helpers;

public static class JwtHelper
{
    public static JwtClaimModel BuildClaims(Guid userId, IEnumerable<AppAccess> allowedApps)
    {
        return new JwtClaimModel
        {
            UserId = userId.ToString(),
            Apps = [.. allowedApps.Select(a => a.AppId.ToString())]
        };
    }
}



