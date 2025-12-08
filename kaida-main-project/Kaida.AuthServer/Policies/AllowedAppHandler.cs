using Microsoft.AspNetCore.Authorization;

namespace Kaida.AuthServer.Policies
{
    public class AllowedAppHandler : AuthorizationHandler<AllowedAppRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AllowedAppRequirement requirement)
        {
            var appsClaim = context.User.FindFirst("apps")?.Value;

            if (appsClaim != null && appsClaim.Split(',').Contains(requirement.RequiredAppId))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
