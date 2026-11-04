using MovieTicketBooking.DTOs.Screens;
using MovieTicketBooking.Exceptions;
using MovieTicketBooking.Helpers;
using MovieTicketBooking.Models;
using MovieTicketBooking.Repositories.Interfaces;
using MovieTicketBooking.Services.Interfaces;

namespace MovieTicketBooking.Services;

public class ScreenService(IScreenRepository screens, ITheatreRepository theatres, IShowRepository shows) : IScreenService
{
    public async Task<ScreenResponse> CreateAsync(CreateScreenRequest r)
    {
        _ = await theatres.GetByIdAsync(r.TheatreId) ?? throw new NotFoundException($"Theatre {r.TheatreId} was not found.");
        var name = r.ScreenName.Trim();
        if (await screens.NameExistsAsync(r.TheatreId, name))
            throw new ConflictException($"A screen named '{name}' already exists in this theatre.");

        var screen = new Screen { TheatreId = r.TheatreId, ScreenName = name };
        await screens.AddAsync(screen);
        return (await screens.GetByIdAsync(screen.ScreenId))!.ToResponse();
    }

    public async Task<List<ScreenResponse>> GetAllAsync(int? theatreId) =>
        (await screens.GetAllAsync(theatreId)).Select(s => s.ToResponse()).ToList();

    public async Task<ScreenResponse> GetByIdAsync(int id) =>
        (await screens.GetByIdAsync(id) ?? throw new NotFoundException($"Screen {id} was not found.")).ToResponse();

    public async Task<List<ScreenResponse>> GetByTheatreAsync(int theatreId)
    {
        _ = await theatres.GetByIdAsync(theatreId) ?? throw new NotFoundException($"Theatre {theatreId} was not found.");
        return await GetAllAsync(theatreId);
    }

    public async Task<ScreenResponse> UpdateAsync(int id, UpdateScreenRequest r)
    {
        var screen = await screens.GetByIdAsync(id) ?? throw new NotFoundException($"Screen {id} was not found.");
        var name = r.ScreenName.Trim();
        if (await screens.NameExistsAsync(screen.TheatreId, name, id))
            throw new ConflictException($"A screen named '{name}' already exists in this theatre.");
        screen.ScreenName = name;
        await screens.UpdateAsync(screen);
        return screen.ToResponse();
    }

    public async Task DeleteAsync(int id)
    {
        var screen = await screens.GetByIdAsync(id) ?? throw new NotFoundException($"Screen {id} was not found.");
        if (await shows.ScreenHasShowsAsync(id))
            throw new ConflictException("This screen has scheduled shows and cannot be deleted.");
        await screens.DeleteAsync(screen); // its seats (no show history) are removed by cascade
    }
}
