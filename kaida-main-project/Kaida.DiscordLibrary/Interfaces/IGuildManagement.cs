using Kaida.Discord.Models.Models;

namespace Kaida.Discord.Library.Interfaces
{
    public interface IGuildManagement
    {
        Task<GuildMember?> FetchGuildMemberAsync(ulong guildId, ulong userId);
        
    }
}
