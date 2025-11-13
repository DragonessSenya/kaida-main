namespace Kaida.AuthServer.Models
{
    public class User
    {
        public required string Id { get; set; }
        public required string UserName { get; set; }
        public required string Password { get; set; }
        public IEnumerable<string> Apps { get; set; } = new List<string>();
        public string PasswordHash { get; internal set; }
    }
}
