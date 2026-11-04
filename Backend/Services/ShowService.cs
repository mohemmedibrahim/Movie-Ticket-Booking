using MovieTicketBooking.DTOs;
using MovieTicketBooking.DTOs.Shows;
using MovieTicketBooking.Enums;
using MovieTicketBooking.Exceptions;
using MovieTicketBooking.Helpers;
using MovieTicketBooking.Models;
using MovieTicketBooking.Repositories.Interfaces;
using MovieTicketBooking.Services.Interfaces;

namespace MovieTicketBooking.Services;

public class ShowService(
    IShowRepository shows, IMovieRepository movies, IScreenRepository screens, ISeatRepository seats) : IShowService
{
    public async Task<ShowResponse> CreateAsync(CreateShowRequest r)
    {
        _ = await movies.GetByIdAsync(r.MovieId) ?? throw new NotFoundException($"Movie {r.MovieId} was not found.");
        _ = await screens.GetByIdAsync(r.ScreenId) ?? throw new NotFoundException($"Screen {r.ScreenId} was not found.");

        var screenSeats = await seats.GetByScreenAsync(r.ScreenId);
        if (screenSeats.Count == 0)
            throw new BadRequestException("This screen has no seats. Create seats before scheduling a show.");

        var start = r.StartTime!.Value;
        var end = r.EndTime!.Value;
        if (await shows.HasOverlapAsync(r.ScreenId, start, end))
            throw new ConflictException("The screen already has a show scheduled during this time.");

        var show = new Show
        {
            MovieId = r.MovieId, ScreenId = r.ScreenId, StartTime = start, EndTime = end, TicketPrice = r.TicketPrice,
            // One ShowSeat per physical seat, initially Available.
            ShowSeats = screenSeats.Select(s => new ShowSeat { SeatId = s.SeatId, Status = SeatStatus.Available }).ToList()
        };
        await shows.AddAsync(show);

        return (await shows.GetByIdAsync(show.ShowId))!.ToResponse();
    }

    public async Task<PagedResponse<ShowResponse>> GetAllAsync(
        int pageNumber, int pageSize, DateOnly? date, int? movieId, int? theatreId)
    {
        (pageNumber, pageSize) = Paging.Normalize(pageNumber, pageSize);
        var (items, total) = await shows.GetPagedAsync(pageNumber, pageSize, date, movieId, theatreId);
        return new PagedResponse<ShowResponse>
        {
            Items = items.Select(s => s.ToResponse()).ToList(),
            PageNumber = pageNumber, PageSize = pageSize, TotalCount = total
        };
    }

    public async Task<ShowResponse> GetByIdAsync(int id) =>
        (await shows.GetByIdAsync(id) ?? throw new NotFoundException($"Show {id} was not found.")).ToResponse();

    public async Task<List<ShowResponse>> GetByMovieAsync(int movieId) =>
        (await shows.GetByMovieAsync(movieId)).Select(s => s.ToResponse()).ToList();

    public async Task<List<ShowResponse>> GetByTheatreAsync(int theatreId) =>
        (await shows.GetByTheatreAsync(theatreId)).Select(s => s.ToResponse()).ToList();

    public async Task<List<ShowResponse>> SearchByDateAsync(DateOnly date) =>
        (await shows.GetByDateAsync(date)).Select(s => s.ToResponse()).ToList();

    public async Task<ShowResponse> UpdateAsync(int id, UpdateShowRequest r)
    {
        var show = await shows.GetByIdAsync(id) ?? throw new NotFoundException($"Show {id} was not found.");
        _ = await movies.GetByIdAsync(r.MovieId) ?? throw new NotFoundException($"Movie {r.MovieId} was not found.");

        var start = r.StartTime!.Value;
        var end = r.EndTime!.Value;
        if (await shows.HasOverlapAsync(show.ScreenId, start, end, id))
            throw new ConflictException("The screen already has a show scheduled during this time.");

        show.MovieId = r.MovieId;
        show.StartTime = start;
        show.EndTime = end;
        show.TicketPrice = r.TicketPrice; // affects new bookings only; existing bookings keep their captured price
        await shows.UpdateAsync(show);

        return (await shows.GetByIdAsync(id))!.ToResponse();
    }

    public async Task DeleteAsync(int id)
    {
        var show = await shows.GetByIdAsync(id) ?? throw new NotFoundException($"Show {id} was not found.");
        if (await shows.HasBookingsAsync(id))
            throw new ConflictException("This show has bookings and cannot be deleted.");
        await shows.DeleteAsync(show);
    }

    public Task<List<ShowSeatResponse>> GetSeatsAsync(int showId) => LoadSeatsAsync(showId, null);
    public Task<List<ShowSeatResponse>> GetAvailableSeatsAsync(int showId) => LoadSeatsAsync(showId, SeatStatus.Available);
    public Task<List<ShowSeatResponse>> GetBookedSeatsAsync(int showId) => LoadSeatsAsync(showId, SeatStatus.Booked);

    private async Task<List<ShowSeatResponse>> LoadSeatsAsync(int showId, SeatStatus? status)
    {
        var show = await shows.GetByIdAsync(showId) ?? throw new NotFoundException($"Show {showId} was not found.");
        var showSeats = await shows.GetShowSeatsAsync(showId, status);
        return showSeats.Select(ss => ss.ToResponse(show.TicketPrice)).ToList();
    }
}
