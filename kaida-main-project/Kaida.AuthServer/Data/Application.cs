using System.ComponentModel.DataAnnotations;
using Kaida.AuthServer.Entities;

namespace Kaida.AuthServer.Data;

/// <summary>
/// Represents an application that users can access via the AuthServer.
/// Each app has a unique ID and a display name.
/// </summary>
public class Application(
    Guid id,
    string name
)
{

    /// <summary>
    /// Primary key of the application.
    /// </summary>
    [Key]
    public Guid Id { get; init; } = id;

    /// <summary>
    /// Name of the application.
    /// </summary>
    [Required, MaxLength(100)]
    public string Name { get; init; } = name;

    /// <summary>
    /// Navigation property for all user accesses related to this application.
    /// </summary>
    public virtual ICollection<UserAccess> UserAccesses { get; init; } = new List<UserAccess>();
}