using System.ComponentModel.DataAnnotations;

namespace Kaida.AuthServer.Entities
{
    public class AppAccess
    {
        public required Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public required Guid AppId { get; set; }
        public App App { get; set; } = null!;
    }
}
