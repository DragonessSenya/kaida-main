namespace Kaida.Shared.Models
{
    public class RefreshTokenRequest
    {
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }

        public string Token { get; set; } = null!;
        public Guid UserId { get; set; }
    }
}
