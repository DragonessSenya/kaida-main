
using Microsoft.VisualBasic.FileIO;
using System.Text.RegularExpressions;

namespace Kaida.Discord.Library.Utils
{
    public class CdnUrlBuilderUtil()
    {
        public string GetUserAssetUrl(DiscordUserAssetType type, string hash, string? user_id = null, string? guild_id = null, int size = 1024)
        {
            var fileType = FileTypeChecker(hash);
            return $"https://cdn.discordapp.com/{type}/{user_id}/{hash}.{fileType}";
        }

        public string GetGuildAssetUrl(DiscordGuildAssetType type, string hash,string? guild_id = null, int size = 1024)
        {
            var fileType = FileTypeChecker(hash);
            return $"https://cdn.discordapp.com/{type}/{guild_id}/{hash}.{fileType}";
        }

        public string FileTypeChecker(string hash)
        {
            var regex = "^a_";
            return Regex.IsMatch(hash, regex) ? "gif" : "png";
        }
    }
}
