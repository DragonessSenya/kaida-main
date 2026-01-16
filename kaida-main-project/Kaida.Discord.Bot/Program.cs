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

            var url = client.User.GetUserAvatarUrl(95277742110539776);
            Console.WriteLine(url);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        Console.WriteLine("this is done");
    }
}