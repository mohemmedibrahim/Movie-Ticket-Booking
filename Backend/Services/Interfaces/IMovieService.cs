using MovieTicketBooking.DTOs;
using MovieTicketBooking.DTOs.Movies;

namespace MovieTicketBooking.Services.Interfaces;

public interface IMovieService
{
    Task<MovieResponse> CreateAsync(CreateMovieRequest request);
    Task<PagedResponse<MovieResponse>> GetAllAsync(int pageNumber, int pageSize, string? title);
    Task<MovieResponse> GetByIdAsync(int id);
    Task<MovieResponse> UpdateAsync(int id, UpdateMovieRequest request);
    Task DeleteAsync(int id);
    Task<List<MovieResponse>> SearchAsync(string title);
}
