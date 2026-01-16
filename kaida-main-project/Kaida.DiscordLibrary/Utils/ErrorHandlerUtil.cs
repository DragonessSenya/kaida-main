using Kaida.Discord.Library.Exceptions;

namespace Kaida.DiscordLibrary.Utils
{
    public static class ErrorHandlerUtil
    {
        public static void HandleResponse(HttpResponseMessage response)
        {
            switch (response.StatusCode)
            {
                case System.Net.HttpStatusCode.NotFound:
                    throw new DiscordLibraryException("Resource not found", response);
                default:
                    throw new DiscordLibraryException("Unexpected status code: {response.StatusCode}", response);
            }
        }
    }
}
