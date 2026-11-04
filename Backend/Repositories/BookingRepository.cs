using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MovieTicketBooking.Data;
using MovieTicketBooking.Enums;
using MovieTicketBooking.Models;
using MovieTicketBooking.Repositories.Interfaces;

namespace MovieTicketBooking.Repositories;

public class BookingRepository(ApplicationDbContext db) : IBookingRepository
{
    private IQueryable<Booking> WithSeats() =>
        db.Bookings.Include(b => b.BookingSeats).ThenInclude(bs => bs.Seat);

    public Task<IDbContextTransaction> BeginTransactionAsync() => db.Database.BeginTransactionAsync();

    public Task<List<ShowSeat>> GetShowSeatsAsync(int showId, IEnumerable<int> seatIds)
    {
        var ids = seatIds.ToList();
        return db.ShowSeats.Include(ss => ss.Seat)
            .Where(ss => ss.ShowId == showId && ids.Contains(ss.SeatId)).ToListAsync();
    }

    public async Task AddAsync(Booking booking)
    {
        db.Bookings.Add(booking);
        await db.SaveChangesAsync();
    }

    public Task SaveChangesAsync() => db.SaveChangesAsync();

    public Task<Booking?> GetByIdAsync(int id) => WithSeats().FirstOrDefaultAsync(b => b.BookingId == id);

    public Task<List<Booking>> GetAllAsync(int? showId, BookingStatus? status)
    {
        var query = WithSeats().AsNoTracking().AsQueryable();
        if (showId.HasValue) query = query.Where(b => b.ShowId == showId.Value);
        if (status.HasValue) query = query.Where(b => b.Status == status.Value);
        return query.OrderByDescending(b => b.BookingDate).ToListAsync();
    }

    public Task<List<Booking>> GetByShowAsync(int showId) =>
        WithSeats().AsNoTracking().Where(b => b.ShowId == showId).OrderByDescending(b => b.BookingDate).ToListAsync();

    public Task<List<Booking>> GetByUserAsync(string userId) =>
        WithSeats().AsNoTracking().Where(b => b.UserId == userId).OrderByDescending(b => b.BookingDate).ToListAsync();
}
