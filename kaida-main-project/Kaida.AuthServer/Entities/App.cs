using System.ComponentModel.DataAnnotations;

namespace Kaida.AuthServer.Entities
{
    public class App
    {
        public required Guid AppId { get; set; }
        [MaxLength(50)]
        public required string AppName { get; set; }

       
    }
}