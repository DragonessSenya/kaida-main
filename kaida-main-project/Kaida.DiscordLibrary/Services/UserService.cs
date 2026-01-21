using Kaida.Discord.Library.Interfaces;
using Kaida.Discord.Library.Utils;
using Kaida.Discord.Models.Models;
using Kaida.DiscordLibrary.Utils;

namespace Kaida.Discord.Library.Services
{
    public class UserService(HttpClient httpClient) : IUserManagement
    {
        private readonly HttpClient _httpClient = httpClient;

        public async Task<User?> FetchUserAsync(ulong userId)
        {
            try
            {
                var response =
                    await _httpClient.GetAsync($"https://discord.com/api/v10/users/{userId}");

                if (!response.IsSuccessStatusCode)
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        ErrorHandlerUtil.HandleResponse(response);
                    }
                }
                else
                {
                    var options = new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var json = await response.Content.ReadAsStringAsync();
                    return System.Text.Json.JsonSerializer.Deserialize<User>(json, options);
                }
            }
            catch (HttpRequestException)
            {
                // throw new DiscordLibraryException("Network error while fetching guild member", e.StatusCode);
            }



            return null;

        }

        public Task BanUserAsync(ulong guildId, ulong userId, string reason = "No Reason Given")
        {
            throw new NotImplementedException();
        }


        public string GetUserAvatarUrl(ulong userId)
        {
            var user = FetchUserAsync(userId).Result;
            CdnUrlBuilderUtil urlBuilder = new CdnUrlBuilderUtil();
            if (user != null && user.Avatar != null)
            {
                return urlBuilder.GetUserAssetUrl(DiscordUserAssetType.avatars, user.Avatar, user.Id);
            }

            return string.Empty;
        }

    }

    
}
