namespace Kaida.Discord.Library.Exceptions
{
    public class DiscordLibraryException : Exception
    {
        public DiscordLibraryException(string networkErrorWhileFetchingGuildMember, HttpResponseMessage responseMessage)
        {
            Console.WriteLine(networkErrorWhileFetchingGuildMember + " - " + responseMessage);
        }
    }
}
