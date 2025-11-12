using System.ComponentModel.DataAnnotations;

namespace Kaida.AuthServer.Models;

/// <summary>
/// Represents a login request sent by a client to the AuthServer.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// The username of the user attempting to log in.
    /// </summary>
    [Required]
    public required string Username { get; set; }

    /// <summary>
    /// The password of the user attempting to log in.
    /// </summary>
    [Required]
    public required string Password { get; set; }

    /// <summary>
    /// The ID of the application the user wants to access.
    /// </summary>
    [Required]
    public required Guid AppId { get; set; }
}