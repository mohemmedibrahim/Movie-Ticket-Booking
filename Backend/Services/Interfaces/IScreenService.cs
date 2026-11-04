using MovieTicketBooking.DTOs.Screens;

namespace MovieTicketBooking.Services.Interfaces;

public interface IScreenService
{
    Task<ScreenResponse> CreateAsync(CreateScreenRequest request);
    Task<List<ScreenResponse>> GetAllAsync(int? theatreId);
    Task<ScreenResponse> GetByIdAsync(int id);
    Task<List<ScreenResponse>> GetByTheatreAsync(int theatreId);
    Task<ScreenResponse> UpdateAsync(int id, UpdateScreenRequest request);
    Task DeleteAsync(int id);
}
