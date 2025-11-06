using System.ComponentModel.DataAnnotations;
using Kaida.AuthServer.Data;
using Microsoft.AspNetCore.Identity;

namespace Kaida.AuthServer.Entities;

/// <summary>
/// Represents the access a user has to a specific application.
/// Controls which apps a user can access and at what access level.
/// </summary>
public class UserAccess
{
    public UserAccess(int id, string userId, Guid appId, string? accessLevel)
    {
        Id = id;
        UserId = userId;
        AppId = appId;
        AccessLevel = accessLevel;
    }

    public UserAccess()
    {
        
    }

    /// <summary>
    /// The primary key of the UserAccess entry.
    /// </summary>
    [Key]
    public int Id { get; init; }

    /// <summary>
    /// The ID of the user (matches IdentityUser.Id).
    /// </summary>
    [Required, MaxLength(450)]
    public required string UserId { get; init; }

    /// <summary>
    /// The ID of the application the user has access to.
    /// </summary>
    public required Guid AppId { get; init; }

    /// <summary>
    /// Optional access level for the user in this app (e.g., Admin, Read, Write).
    /// </summary>
    [Required, MaxLength(50)]
    public string? AccessLevel { get; init; }

    /// <summary>
    /// Navigation property to the <see cref="IdentityUser"/>.
    /// </summary>
    public IdentityUser User { get; init; }

    /// <summary>
    /// Navigation property to the <see cref="Application"/> entity.
    /// </summary>
    public Application App { get; init; }
}