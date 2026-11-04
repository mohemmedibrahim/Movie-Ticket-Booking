using MovieTicketBooking.Models;

namespace MovieTicketBooking.Services.Interfaces;

public interface ITokenService
{
    (string Token, DateTime ExpiresAtUtc) CreateToken(ApplicationUser user, IEnumerable<string> roles);
}
