using Microsoft.EntityFrameworkCore;
using MovieTicketBooking.Data;
using MovieTicketBooking.Models;
using MovieTicketBooking.Repositories.Interfaces;

namespace MovieTicketBooking.Repositories;

public class TheatreRepository(ApplicationDbContext db) : ITheatreRepository
{
    public Task<List<Theatre>> GetAllAsync(int pageNumber, int pageSize) =>
        db.Theatres.AsNoTracking().OrderBy(t => t.Name)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

    public Task<Theatre?> GetByIdAsync(int id) => db.Theatres.FirstOrDefaultAsync(t => t.TheatreId == id);

    public async Task AddAsync(Theatre theatre)
    {
        db.Theatres.Add(theatre);
        await db.SaveChangesAsync();
    }

    public Task UpdateAsync(Theatre theatre) => db.SaveChangesAsync();

    public async Task DeleteAsync(Theatre theatre)
    {
        db.Theatres.Remove(theatre);
        await db.SaveChangesAsync();
    }

    public Task<bool> HasScreensAsync(int theatreId) => db.Screens.AnyAsync(s => s.TheatreId == theatreId);
}
