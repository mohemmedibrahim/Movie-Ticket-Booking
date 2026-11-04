using MovieTicketBooking.Models;

namespace MovieTicketBooking.Repositories.Interfaces;

public interface IMovieRepository
{
    Task<(List<Movie> Items, int Total)> GetPagedAsync(int pageNumber, int pageSize, string? title);
    Task<List<Movie>> SearchByTitleAsync(string title);
    Task<Movie?> GetByIdAsync(int id);
    Task AddAsync(Movie movie);
    Task UpdateAsync(Movie movie);
    Task DeleteAsync(Movie movie);
    Task<bool> HasShowsAsync(int movieId);
}
