using Microsoft.EntityFrameworkCore;
using MovieTicketBooking.Data;
using MovieTicketBooking.Models;
using MovieTicketBooking.Repositories.Interfaces;

namespace MovieTicketBooking.Repositories;

public class ScreenRepository(ApplicationDbContext db) : IScreenRepository
{
    public Task<List<Screen>> GetAllAsync(int? theatreId)
    {
        var query = db.Screens.AsNoTracking().Include(s => s.Theatre).AsQueryable();
        if (theatreId.HasValue) query = query.Where(s => s.TheatreId == theatreId.Value);
        return query.OrderBy(s => s.TheatreId).ThenBy(s => s.ScreenName).ToListAsync();
    }

    public Task<Screen?> GetByIdAsync(int id) =>
        db.Screens.Include(s => s.Theatre).FirstOrDefaultAsync(s => s.ScreenId == id);

    public Task<bool> NameExistsAsync(int theatreId, string screenName, int? excludeScreenId = null) =>
        db.Screens.AnyAsync(s => s.TheatreId == theatreId && s.ScreenName == screenName
                                 && (excludeScreenId == null || s.ScreenId != excludeScreenId));

    public async Task AddAsync(Screen screen)
    {
        db.Screens.Add(screen);
        await db.SaveChangesAsync();
    }

    public Task UpdateAsync(Screen screen) => db.SaveChangesAsync();

    public async Task DeleteAsync(Screen screen)
    {
        db.Screens.Remove(screen);
        await db.SaveChangesAsync();
    }
}
