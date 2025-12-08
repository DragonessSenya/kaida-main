namespace Kaida.AuthServer.Models
{
    public class FailedLoginAttemptsDto
    {
        public string? IpAddress { get; set; } = string.Empty;
        public int? FailedLoginAttempts { get; set; } = 0;
    }
}
