using MovieTicketBooking.DTOs.Theatres;
using MovieTicketBooking.Exceptions;
using MovieTicketBooking.Helpers;
using MovieTicketBooking.Models;
using MovieTicketBooking.Repositories.Interfaces;
using MovieTicketBooking.Services.Interfaces;

namespace MovieTicketBooking.Services;

public class TheatreService(ITheatreRepository theatres) : ITheatreService
{
    public async Task<TheatreResponse> CreateAsync(CreateTheatreRequest r)
    {
        var t = new Theatre { Name = r.Name.Trim(), Location = r.Location.Trim() };
        await theatres.AddAsync(t);
        return t.ToResponse();
    }

    public async Task<List<TheatreResponse>> GetAllAsync(int pageNumber, int pageSize)
    {
        (pageNumber, pageSize) = Paging.Normalize(pageNumber, pageSize);
        return (await theatres.GetAllAsync(pageNumber, pageSize)).Select(t => t.ToResponse()).ToList();
    }

    public async Task<TheatreResponse> GetByIdAsync(int id) =>
        (await theatres.GetByIdAsync(id) ?? throw new NotFoundException($"Theatre {id} was not found.")).ToResponse();

    public async Task<TheatreResponse> UpdateAsync(int id, UpdateTheatreRequest r)
    {
        var t = await theatres.GetByIdAsync(id) ?? throw new NotFoundException($"Theatre {id} was not found.");
        t.Name = r.Name.Trim();
        t.Location = r.Location.Trim();
        await theatres.UpdateAsync(t);
        return t.ToResponse();
    }

    public async Task DeleteAsync(int id)
    {
        var t = await theatres.GetByIdAsync(id) ?? throw new NotFoundException($"Theatre {id} was not found.");
        if (await theatres.HasScreensAsync(id))
            throw new ConflictException("This theatre has screens and cannot be deleted.");
        await theatres.DeleteAsync(t);
    }
}
