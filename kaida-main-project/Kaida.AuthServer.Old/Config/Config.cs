using Duende.IdentityServer.Models;

namespace Kaida.AuthServer.Config;

public static class Config
{
    // Identity resources (claims about the user)
    public static IEnumerable<IdentityResource> IdentityResources =>
        new List<IdentityResource>
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email()
        };

    // API scopes (what APIs a client can access)
    public static IEnumerable<ApiScope> ApiScopes =>
        new List<ApiScope>
        {
            new ApiScope("dashboard_api", "Dashboard API"),
            new ApiScope("trello_api", "Trello Clone API")
        };

    // Clients (apps that request tokens)
    public static IEnumerable<Client> Clients =>
        new List<Client>
        {
            // Dashboard app
            new Client
            {
                ClientId = "dashboard",
                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                ClientSecrets = { new Secret("super-secret-dashboard".Sha256()) },
                AllowedScopes = { "dashboard_api", "openid", "profile", "email" }
            },

            // Trello clone app
            new Client
            {
                ClientId = "trello",
                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                ClientSecrets = { new Secret("super-secret-trello".Sha256()) },
                AllowedScopes = { "trello_api", "openid", "profile", "email" }
            },

             // DemoApp (added to satisfy default fallback)
            new Client
            {
                ClientId = "DemoApp",
                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                ClientSecrets = { new Secret("super-secret-demoapp".Sha256()) },
                // adjust allowed scopes to match what DemoApp needs
                AllowedScopes = { "dashboard_api", "openid", "profile", "email" }
            }
        };
}