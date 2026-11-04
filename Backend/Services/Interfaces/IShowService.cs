using MovieTicketBooking.DTOs;
using MovieTicketBooking.DTOs.Shows;

namespace MovieTicketBooking.Services.Interfaces;

public interface IShowService
{
    Task<ShowResponse> CreateAsync(CreateShowRequest request);
    Task<PagedResponse<ShowResponse>> GetAllAsync(int pageNumber, int pageSize, DateOnly? date, int? movieId, int? theatreId);
    Task<ShowResponse> GetByIdAsync(int id);
    Task<List<ShowResponse>> GetByMovieAsync(int movieId);
    Task<List<ShowResponse>> GetByTheatreAsync(int theatreId);
    Task<List<ShowResponse>> SearchByDateAsync(DateOnly date);
    Task<ShowResponse> UpdateAsync(int id, UpdateShowRequest request);
    Task DeleteAsync(int id);

    // ShowSeats (managed through shows)
    Task<List<ShowSeatResponse>> GetSeatsAsync(int showId);
    Task<List<ShowSeatResponse>> GetAvailableSeatsAsync(int showId);
    Task<List<ShowSeatResponse>> GetBookedSeatsAsync(int showId);
}
