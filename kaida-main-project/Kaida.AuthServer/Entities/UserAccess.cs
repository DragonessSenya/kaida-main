using Microsoft.AspNetCore.Identity;

namespace Kaida.AuthServer.Entities
{
    /// <summary>
    /// Represents a link between a user and an application, defining access level.
    /// </summary>
    public class UserAccess
    {
        public string UserId { get; set; } = string.Empty;
        public Guid AppId { get; set; }
        public string AccessLevel { get; set; } = "User";
        public Application? App { get; set; } = null!;
    }
}
