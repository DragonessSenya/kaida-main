using System.ComponentModel.DataAnnotations;

namespace Kaida.AuthServer.Entities
{
    public class FailedLoginAttempts
    {
        [Key]
        public required int Id { get; set; }
        [MaxLength(50)]
        
        public Guid? UserId { get; set; }
        [MaxLength(45)]
        public string? IpAddress { get; set; } = string.Empty;
        public int? LoginAttempts { get; set; }



    }
}
