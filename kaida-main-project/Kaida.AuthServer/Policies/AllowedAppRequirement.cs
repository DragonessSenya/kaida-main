using Microsoft.AspNetCore.Authorization;

namespace Kaida.AuthServer.Policies;

public class AllowedAppRequirement : IAuthorizationRequirement
{
    public string RequiredAppId { get; }
    public AllowedAppRequirement(string requiredAppId)
    {
        RequiredAppId = requiredAppId;
    }
}

