using System.ComponentModel.DataAnnotations;

namespace Kaida.AuthServer.Entities
{
    public class User
    {
        [MaxLength(50)]
        public required Guid UserId { get; set; } = Guid.NewGuid();
        [MaxLength(50)]
        public required string UserName { get; set; }
        [MaxLength(50)]
        public required string Password { get; set; }
    }
}
