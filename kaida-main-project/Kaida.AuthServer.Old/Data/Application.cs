using System.ComponentModel.DataAnnotations;
using Kaida.AuthServer.Entities;

namespace Kaida.AuthServer.Data;

/// <summary>
/// Represents an application that users can access via the AuthServer.
/// Each app has a unique ID and a display name.
/// </summary>
public class Application
{
    public Application(Guid id, string name)
    {
        Id = id;
        Name = name;
        UserAccesses = new List<UserAccess>();
    }

    // Parameterless constructor required by EF Core for materialization
    protected Application()
    {
        UserAccesses = new List<UserAccess>();
        Name = string.Empty;
    }

    /// <summary>
    /// Primary key of the application.
    /// </summary>
    [Key]
    public Guid Id { get; init; }

    /// <summary>
    /// Name of the application.
    /// </summary>
    [Required, MaxLength(100)]
    public string Name { get; init; }

    /// <summary>
    /// Navigation property for all user accesses related to this application.
    /// Made settable and virtual so EF Core can materialize and (optionally) proxy it.
    /// </summary>
    public virtual ICollection<UserAccess> UserAccesses { get; set; }
}