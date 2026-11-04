using Microsoft.EntityFrameworkCore;
using MovieTicketBooking.Data;
using MovieTicketBooking.Models;
using MovieTicketBooking.Repositories.Interfaces;

namespace MovieTicketBooking.Repositories;

public class MovieRepository(ApplicationDbContext db) : IMovieRepository
{
    public async Task<(List<Movie> Items, int Total)> GetPagedAsync(int pageNumber, int pageSize, string? title)
    {
        var query = db.Movies.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(title))
            query = query.Where(m => m.Title.Contains(title));

        var total = await query.CountAsync();
        var items = await query.OrderBy(m => m.Title)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, total);
    }

    public Task<List<Movie>> SearchByTitleAsync(string title) =>
        db.Movies.AsNoTracking().Where(m => m.Title.Contains(title)).OrderBy(m => m.Title).ToListAsync();

    public Task<Movie?> GetByIdAsync(int id) => db.Movies.FirstOrDefaultAsync(m => m.MovieId == id);

    public async Task AddAsync(Movie movie)
    {
        db.Movies.Add(movie);
        await db.SaveChangesAsync();
    }

    public Task UpdateAsync(Movie movie) => db.SaveChangesAsync();

    public async Task DeleteAsync(Movie movie)
    {
        db.Movies.Remove(movie);
        await db.SaveChangesAsync();
    }

    public Task<bool> HasShowsAsync(int movieId) => db.Shows.AnyAsync(s => s.MovieId == movieId);
}
