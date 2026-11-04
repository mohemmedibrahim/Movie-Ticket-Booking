using MovieTicketBooking.DTOs.Bookings;

namespace MovieTicketBooking.Services.Interfaces;

public interface IBookingService
{
    Task<BookingResponse> CreateAsync(CreateBookingRequest request, string userId, string accountEmail);
    Task<List<BookingResponse>> GetAllAsync(int? showId, string? status);
    Task<List<BookingResponse>> GetMyBookingsAsync(string userId);
    Task<BookingResponse> GetByIdAsync(int id, string userId, bool isAdmin);
    Task<List<BookingResponse>> GetByShowAsync(int showId);
    Task<BookingResponse> CancelAsync(int id, string userId, bool isAdmin);
}
