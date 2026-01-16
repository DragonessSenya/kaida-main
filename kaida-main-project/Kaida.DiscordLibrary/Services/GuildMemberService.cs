using Kaida.Discord.Library.Interfaces;
using Kaida.Discord.Library.Models;
using Kaida.DiscordLibrary.Utils;

namespace Kaida.DiscordLibrary.Services
{
    public class GuildMemberService(HttpClient httpClient) : IGuildManagement
    {
        private readonly HttpClient _httpClient = httpClient;

        public async Task<GuildMember?> FetchGuildMemberAsync(ulong guildId, ulong userId)
        {
            try
            {
              var  response =
                    await _httpClient.GetAsync($"https://discord.com/api/v10/guilds/{guildId}/members/{userId}");

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
                  return System.Text.Json.JsonSerializer.Deserialize<GuildMember>(json, options);
              }
            }
            catch (HttpRequestException)
            {
               // throw new DiscordLibraryException("Network error while fetching guild member", e.StatusCode);
            }



            return null;

        }

    }


}
