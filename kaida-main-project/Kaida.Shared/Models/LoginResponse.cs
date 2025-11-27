using System.ComponentModel.DataAnnotations;

namespace Kaida.Shared.Models;

/// <summary>
/// Represents a login response returned by the AuthServer upon successful authentication.
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// The JWT token issued for the user to access the requested application.
    /// </summary>
    [Required]
    public required string AccessToken { get; set; }

    /// <summary>
    /// The Refresh token issued for the user to access the requested application.
    /// </summary>
    [Required]
    public required string RefreshToken { get; set; }

    /// <summary>
    /// The UTC expiration time of the JWT token.
   /// </summary>
    [Required]
    public required DateTime Expiration { get; set; }
}