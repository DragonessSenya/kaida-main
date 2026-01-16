
namespace Kaida.Discord.Library.Models
{
    public class User()
    {
        public required string Id { get; set; }
        public required string Username { get; set; }
        public required string Discriminator { get; set; }
        public string? GlobalName { get; set; }
        public string? Avatar { get; set; }
        public string? Banner { get; set; }
    }
}
