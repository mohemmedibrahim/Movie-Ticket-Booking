using MovieTicketBooking.DTOs.Seats;
using MovieTicketBooking.Exceptions;
using MovieTicketBooking.Helpers;
using MovieTicketBooking.Models;
using MovieTicketBooking.Repositories.Interfaces;
using MovieTicketBooking.Services.Interfaces;

namespace MovieTicketBooking.Services;

public class SeatService(ISeatRepository seats, IScreenRepository screens, IShowRepository shows) : ISeatService
{
    private const int MaxSeatsPerScreen = 1000;

    // ShowSeats are generated only when a show is created, so adding seats afterwards would
    // leave existing shows without them. V1 therefore blocks this instead of guessing.
    private async Task EnsureScreenIsEditableAsync(int screenId)
    {
        _ = await screens.GetByIdAsync(screenId) ?? throw new NotFoundException($"Screen {screenId} was not found.");
        if (await shows.ScreenHasShowsAsync(screenId))
            throw new ConflictException("Seats cannot be added to a screen that already has scheduled shows.");
    }

    public async Task<SeatResponse> CreateAsync(CreateSeatRequest r)
    {
        await EnsureScreenIsEditableAsync(r.ScreenId);
        var row = r.RowName.Trim().ToUpperInvariant();
        if (await seats.ExistsAsync(r.ScreenId, row, r.SeatNumber))
            throw new ConflictException($"Seat {row}{r.SeatNumber} already exists on this screen.");

        var seat = new Seat { ScreenId = r.ScreenId, RowName = row, SeatNumber = r.SeatNumber, SeatType = r.SeatType };
        await seats.AddAsync(seat);
        return seat.ToResponse();
    }

    public async Task<List<SeatResponse>> CreateBulkAsync(BulkCreateSeatsRequest r)
    {
        await EnsureScreenIsEditableAsync(r.ScreenId);

        var rowNames = r.Rows.Select(x => x.RowName.Trim().ToUpperInvariant()).ToList();
        if (rowNames.Distinct().Count() != rowNames.Count)
            throw new BadRequestException("Each row name may appear only once in a bulk request.");

        var existing = await seats.GetCoordinatesAsync(r.ScreenId);
        var newSeats = new List<Seat>();

        foreach (var row in r.Rows)
        {
            var rowName = row.RowName.Trim().ToUpperInvariant();
            for (var n = 1; n <= row.SeatCount; n++)
            {
                if (existing.Contains((rowName, n)))
                    throw new ConflictException($"Seat {rowName}{n} already exists on this screen.");
                newSeats.Add(new Seat { ScreenId = r.ScreenId, RowName = rowName, SeatNumber = n, SeatType = row.SeatType });
            }
        }

        if (existing.Count + newSeats.Count > MaxSeatsPerScreen)
            throw new BadRequestException($"A screen cannot have more than {MaxSeatsPerScreen} seats.");

        await seats.AddRangeAsync(newSeats);
        return newSeats.Select(s => s.ToResponse()).ToList();
    }

    public async Task<List<SeatResponse>> GetByScreenAsync(int screenId)
    {
        _ = await screens.GetByIdAsync(screenId) ?? throw new NotFoundException($"Screen {screenId} was not found.");
        return (await seats.GetByScreenAsync(screenId)).Select(s => s.ToResponse()).ToList();
    }

    public async Task<SeatResponse> GetByIdAsync(int id) =>
        (await seats.GetByIdAsync(id) ?? throw new NotFoundException($"Seat {id} was not found.")).ToResponse();

    public async Task<SeatResponse> UpdateAsync(int id, UpdateSeatRequest r)
    {
        var seat = await seats.GetByIdAsync(id) ?? throw new NotFoundException($"Seat {id} was not found.");
        var row = r.RowName.Trim().ToUpperInvariant();
        if (await seats.ExistsAsync(seat.ScreenId, row, r.SeatNumber, id))
            throw new ConflictException($"Seat {row}{r.SeatNumber} already exists on this screen.");

        seat.RowName = row;
        seat.SeatNumber = r.SeatNumber;
        seat.SeatType = r.SeatType;
        await seats.UpdateAsync(seat);
        return seat.ToResponse();
    }

    public async Task DeleteAsync(int id)
    {
        var seat = await seats.GetByIdAsync(id) ?? throw new NotFoundException($"Seat {id} was not found.");
        if (await seats.HasShowHistoryAsync(id))
            throw new ConflictException("This seat is used by shows or bookings and cannot be deleted.");
        await seats.DeleteAsync(seat);
    }
}
