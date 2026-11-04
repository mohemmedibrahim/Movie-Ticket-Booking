using Microsoft.EntityFrameworkCore.Storage;
using MovieTicketBooking.Enums;
using MovieTicketBooking.Models;

namespace MovieTicketBooking.Repositories.Interfaces;

public interface IBookingRepository
{
    Task<IDbContextTransaction> BeginTransactionAsync();
    Task<List<ShowSeat>> GetShowSeatsAsync(int showId, IEnumerable<int> seatIds);
    Task AddAsync(Booking booking);
    Task SaveChangesAsync();
    Task<Booking?> GetByIdAsync(int id);
    Task<List<Booking>> GetAllAsync(int? showId, BookingStatus? status);
    Task<List<Booking>> GetByShowAsync(int showId);
    Task<List<Booking>> GetByUserAsync(string userId);
}
