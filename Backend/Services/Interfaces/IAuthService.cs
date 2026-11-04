using MovieTicketBooking.DTOs.Auth;

namespace MovieTicketBooking.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterCustomerAsync(RegisterRequest request);
    Task<UserResponse> RegisterAdminAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<UserResponse> GetProfileAsync(string userId);
}
