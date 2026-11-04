using Microsoft.EntityFrameworkCore;
using MovieTicketBooking.Data;
using MovieTicketBooking.Enums;
using MovieTicketBooking.Models;
using MovieTicketBooking.Repositories.Interfaces;

namespace MovieTicketBooking.Repositories;

public class ShowRepository(ApplicationDbContext db) : IShowRepository
{
    private IQueryable<Show> WithDetails() =>
        db.Shows.Include(s => s.Movie).Include(s => s.Screen).ThenInclude(sc => sc.Theatre);

    public async Task<(List<Show> Items, int Total)> GetPagedAsync(
        int pageNumber, int pageSize, DateOnly? date, int? movieId, int? theatreId)
    {
        var query = WithDetails().AsNoTracking();

        if (date.HasValue)
        {
            var start = date.Value.ToDateTime(TimeOnly.MinValue);
            var end = start.AddDays(1);
            query = query.Where(s => s.StartTime >= start && s.StartTime < end);
        }
        if (movieId.HasValue) query = query.Where(s => s.MovieId == movieId.Value);
        if (theatreId.HasValue) query = query.Where(s => s.Screen.TheatreId == theatreId.Value);

        var total = await query.CountAsync();
        var items = await query.OrderBy(s => s.StartTime)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, total);
    }

    public Task<List<Show>> GetByMovieAsync(int movieId) =>
        WithDetails().AsNoTracking().Where(s => s.MovieId == movieId).OrderBy(s => s.StartTime).ToListAsync();

    public Task<List<Show>> GetByTheatreAsync(int theatreId) =>
        WithDetails().AsNoTracking().Where(s => s.Screen.TheatreId == theatreId).OrderBy(s => s.StartTime).ToListAsync();

    public Task<List<Show>> GetByDateAsync(DateOnly date)
    {
        var start = date.ToDateTime(TimeOnly.MinValue);
        var end = start.AddDays(1);
        return WithDetails().AsNoTracking()
            .Where(s => s.StartTime >= start && s.StartTime < end).OrderBy(s => s.StartTime).ToListAsync();
    }

    public Task<Show?> GetByIdAsync(int id) => WithDetails().FirstOrDefaultAsync(s => s.ShowId == id);

    // Two intervals overlap when each one starts before the other ends.
    public Task<bool> HasOverlapAsync(int screenId, DateTime start, DateTime end, int? excludeShowId = null) =>
        db.Shows.AnyAsync(s => s.ScreenId == screenId
                               && s.StartTime < end && s.EndTime > start
                               && (excludeShowId == null || s.ShowId != excludeShowId));

    public Task<bool> ScreenHasShowsAsync(int screenId) => db.Shows.AnyAsync(s => s.ScreenId == screenId);

    public Task<bool> HasBookingsAsync(int showId) => db.Bookings.AnyAsync(b => b.ShowId == showId);

    public Task<List<ShowSeat>> GetShowSeatsAsync(int showId, SeatStatus? status)
    {
        var query = db.ShowSeats.AsNoTracking().Include(ss => ss.Seat).Where(ss => ss.ShowId == showId);
        if (status.HasValue) query = query.Where(ss => ss.Status == status.Value);
        return query.OrderBy(ss => ss.Seat.RowName).ThenBy(ss => ss.Seat.SeatNumber).ToListAsync();
    }

    public async Task AddAsync(Show show)
    {
        db.Shows.Add(show);   // ShowSeats in show.ShowSeats are inserted in the same SaveChanges (atomic)
        await db.SaveChangesAsync();
    }

    public Task UpdateAsync(Show show) => db.SaveChangesAsync();

    public async Task DeleteAsync(Show show)
    {
        db.Shows.Remove(show); // ShowSeats are removed by cascade
        await db.SaveChangesAsync();
    }
}
