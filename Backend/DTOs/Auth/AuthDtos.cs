using System.ComponentModel.DataAnnotations;

namespace MovieTicketBooking.DTOs.Auth;

public class RegisterRequest
{
    [Required, MaxLength(100)] public string FullName { get; set; } = string.Empty;
    [Required, EmailAddress, MaxLength(150)] public string Email { get; set; } = string.Empty;
    [Required, RegularExpression(@"^\+?[0-9]{7,14}$", ErrorMessage = "Phone must be 7-14 digits, optionally starting with +.")]
    public string Phone { get; set; } = string.Empty;
    [Required, MinLength(6), MaxLength(100)] public string Password { get; set; } = string.Empty;
}

public class LoginRequest
{
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public DateTime ExpiresAtUtc { get; set; }
    public UserResponse User { get; set; } = new();
}

public class UserResponse
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();
}
