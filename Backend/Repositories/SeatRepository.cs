using Microsoft.EntityFrameworkCore;
using MovieTicketBooking.Data;
using MovieTicketBooking.Models;
using MovieTicketBooking.Repositories.Interfaces;

namespace MovieTicketBooking.Repositories;

public class SeatRepository(ApplicationDbContext db) : ISeatRepository
{
    public Task<List<Seat>> GetByScreenAsync(int screenId) =>
        db.Seats.AsNoTracking().Where(s => s.ScreenId == screenId)
            .OrderBy(s => s.RowName).ThenBy(s => s.SeatNumber).ToListAsync();

    public Task<Seat?> GetByIdAsync(int id) => db.Seats.FirstOrDefaultAsync(s => s.SeatId == id);

    public Task<bool> ExistsAsync(int screenId, string rowName, int seatNumber, int? excludeSeatId = null) =>
        db.Seats.AnyAsync(s => s.ScreenId == screenId && s.RowName == rowName && s.SeatNumber == seatNumber
                               && (excludeSeatId == null || s.SeatId != excludeSeatId));

    public async Task<HashSet<(string Row, int Number)>> GetCoordinatesAsync(int screenId)
    {
        var list = await db.Seats.AsNoTracking().Where(s => s.ScreenId == screenId)
            .Select(s => new { s.RowName, s.SeatNumber }).ToListAsync();
        return list.Select(x => (x.RowName.ToUpperInvariant(), x.SeatNumber)).ToHashSet();
    }

    public async Task AddAsync(Seat seat)
    {
        db.Seats.Add(seat);
        await db.SaveChangesAsync();
    }

    public async Task AddRangeAsync(IEnumerable<Seat> seats)
    {
        db.Seats.AddRange(seats);
        await db.SaveChangesAsync();
    }

    public Task UpdateAsync(Seat seat) => db.SaveChangesAsync();

    public async Task DeleteAsync(Seat seat)
    {
        db.Seats.Remove(seat);
        await db.SaveChangesAsync();
    }

    public async Task<bool> HasShowHistoryAsync(int seatId) =>
        await db.ShowSeats.AnyAsync(ss => ss.SeatId == seatId) ||
        await db.BookingSeats.AnyAsync(bs => bs.SeatId == seatId);
}
