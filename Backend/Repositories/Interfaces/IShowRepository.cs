using MovieTicketBooking.Enums;
using MovieTicketBooking.Models;

namespace MovieTicketBooking.Repositories.Interfaces;

public interface IShowRepository
{
    Task<(List<Show> Items, int Total)> GetPagedAsync(int pageNumber, int pageSize, DateOnly? date, int? movieId, int? theatreId);
    Task<List<Show>> GetByMovieAsync(int movieId);
    Task<List<Show>> GetByTheatreAsync(int theatreId);
    Task<List<Show>> GetByDateAsync(DateOnly date);
    Task<Show?> GetByIdAsync(int id);
    Task<bool> HasOverlapAsync(int screenId, DateTime start, DateTime end, int? excludeShowId = null);
    Task<bool> ScreenHasShowsAsync(int screenId);
    Task<bool> HasBookingsAsync(int showId);
    Task<List<ShowSeat>> GetShowSeatsAsync(int showId, SeatStatus? status);
    Task AddAsync(Show show);
    Task UpdateAsync(Show show);
    Task DeleteAsync(Show show);
}
