using Kaida.Discord.Library.Models;

namespace Kaida.Discord.Library.Interfaces
{
    public interface IUserManagement
    {
        Task BanUserAsync(ulong guildId, ulong userId, string reason = "No Reason Given");

    }
}
