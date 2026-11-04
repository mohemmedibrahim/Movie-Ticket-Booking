using MovieTicketBooking.DTOs.Theatres;

namespace MovieTicketBooking.Services.Interfaces;

public interface ITheatreService
{
    Task<TheatreResponse> CreateAsync(CreateTheatreRequest request);
    Task<List<TheatreResponse>> GetAllAsync(int pageNumber, int pageSize);
    Task<TheatreResponse> GetByIdAsync(int id);
    Task<TheatreResponse> UpdateAsync(int id, UpdateTheatreRequest request);
    Task DeleteAsync(int id);
}
