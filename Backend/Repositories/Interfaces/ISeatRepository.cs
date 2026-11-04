using MovieTicketBooking.Models;

namespace MovieTicketBooking.Repositories.Interfaces;

public interface ISeatRepository
{
    Task<List<Seat>> GetByScreenAsync(int screenId);
    Task<Seat?> GetByIdAsync(int id);
    Task<bool> ExistsAsync(int screenId, string rowName, int seatNumber, int? excludeSeatId = null);
    Task<HashSet<(string Row, int Number)>> GetCoordinatesAsync(int screenId);
    Task AddAsync(Seat seat);
    Task AddRangeAsync(IEnumerable<Seat> seats);
    Task UpdateAsync(Seat seat);
    Task DeleteAsync(Seat seat);
    Task<bool> HasShowHistoryAsync(int seatId);
}
