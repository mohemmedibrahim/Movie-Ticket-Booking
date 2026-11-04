using MovieTicketBooking.DTOs.Seats;

namespace MovieTicketBooking.Services.Interfaces;

public interface ISeatService
{
    Task<SeatResponse> CreateAsync(CreateSeatRequest request);
    Task<List<SeatResponse>> CreateBulkAsync(BulkCreateSeatsRequest request);
    Task<List<SeatResponse>> GetByScreenAsync(int screenId);
    Task<SeatResponse> GetByIdAsync(int id);
    Task<SeatResponse> UpdateAsync(int id, UpdateSeatRequest request);
    Task DeleteAsync(int id);
}
