using System;
using System.Collections.Generic;
using System.Text;
using Kaida.Discord.Library.Models;
using Kaida.Discord.Library.Services;
using Kaida.DiscordLibrary.Services;

namespace Kaida.Discord.Library
{
    public class KaidaClient
    {
        public GuildMemberService Guild { get; }
        public UserService User { get; }


        private KaidaClient(string botToken)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bot", botToken);

            Guild = new GuildMemberService(httpClient);
            User = new UserService(httpClient);
        }

        public static KaidaClient Connect(string botToken)
        {
            return new KaidaClient(botToken);
        }
    }
    

}
