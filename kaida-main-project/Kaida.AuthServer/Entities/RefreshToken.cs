namespace Kaida.AuthServer.Entities
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public string Token { get; set; } = null!;
        public DateTime Expiration { get; set; }
        public bool IsRevoked { get; set; } = false;

        // Navigation property
        public User User { get; set; } = null!;
    }

}
