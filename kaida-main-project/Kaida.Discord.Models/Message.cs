using Kaida.Discord.Models.Models;

namespace Kaida.Discord.Library.Models
{
    public class Message
    {
        public required string channel_id { get; set; }
        public User? author { get; set; }
        public required string content { get; set; }
    }
}