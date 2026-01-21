using Kaida.Discord.Library.Models;
using Kaida.Discord.Library.Utils;
using Kaida.Discord.Library;

namespace Kaida.Discord.Bot.Test;

public class Program
{
    private static async Task Main()
    {
        string? token = "";



        KaidaClient client = KaidaClient.Connect(token);

        try
        {
            // GuildMember? member = await client.Guild.FetchGuildMemberAsync(854747465524445234, 95277742110539776);
            // if (member != null)
            // {
            //    
            //    
            //         Console.WriteLine(
            //             $"MemberId: {member.User.Id} | Username: {member.User.Username} | Avatar: {member.User.Avatar} | Nickname: {member.Nick} | Join date: {member.Joined_At} | Banner: {member.User.Banner} | Role: {member.Roles[0]}");
            //     
            // }

            //var url = client.User.GetUserAvatarUrl(142123374305345536);
            //Console.WriteLine(url);

            var message = await client.Guild.FetchGuildMessageFromChannel(888484257390010418, 1449824863182393526);
            Console.WriteLine(message?.content);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        Console.WriteLine("this is done");
    }
}