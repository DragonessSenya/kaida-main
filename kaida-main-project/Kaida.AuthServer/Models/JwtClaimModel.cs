namespace Kaida.AuthServer.Models
{
    public class JwtClaimModel()
    {
        public required string UserId { get; set; }
        public IEnumerable<string> Apps { get; set; } = new List<string>();
    }
}
