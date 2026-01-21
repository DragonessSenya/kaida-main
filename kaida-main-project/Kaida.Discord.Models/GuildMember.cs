
namespace Kaida.Discord.Models.Models
{
    public class GuildMember()
    {
        public required User User { get; set; }
        public string? Nick { get; set; }
        public string? Avatar { get; set; }
        public string? Banner { get; set; }
        public required  List<string> Roles { get; set; }
        public DateTimeOffset? Joined_At { get; set; }

    }


}
