namespace Kaida.AuthServer.Entities
{
    public class Application
    {
        public Guid AppId { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public ICollection<UserAccess> UserAccesses { get; set; } = new List<UserAccess>();
    }
}
