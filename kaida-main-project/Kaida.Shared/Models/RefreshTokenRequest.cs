namespace Kaida.Shared.Models
{
    public class RefreshTokenRequest
    {
        public string Token { get; set; } = null!;
        public Guid UserId { get; set; }
    }
}
