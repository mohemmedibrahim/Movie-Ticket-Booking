using MovieTicketBooking.DTOs.Bookings;
using MovieTicketBooking.Enums;
using MovieTicketBooking.Exceptions;
using MovieTicketBooking.Helpers;
using MovieTicketBooking.Models;
using MovieTicketBooking.Repositories.Interfaces;
using MovieTicketBooking.Services.Interfaces;

namespace MovieTicketBooking.Services;

public class BookingService(IBookingRepository bookings, IShowRepository shows) : IBookingService
{
    public async Task<BookingResponse> CreateAsync(CreateBookingRequest r, string userId, string accountEmail)
    {
        var show = await shows.GetByIdAsync(r.ShowId) ?? throw new NotFoundException($"Show {r.ShowId} was not found.");

        if (r.SeatIds.Distinct().Count() != r.SeatIds.Count)
            throw new BadRequestException("SeatIds must be distinct.");

        var showSeats = await bookings.GetShowSeatsAsync(r.ShowId, r.SeatIds);
        var missing = r.SeatIds.Except(showSeats.Select(x => x.SeatId)).ToList();
        if (missing.Count > 0)
            throw new BadRequestException($"Seat(s) {string.Join(", ", missing)} do not belong to this show's screen.");

        var unavailable = showSeats.Where(x => x.Status != SeatStatus.Available)
            .Select(x => $"{x.Seat.RowName}{x.Seat.SeatNumber}").ToList();
        if (unavailable.Count > 0)
            throw new ConflictException($"Seat(s) {string.Join(", ", unavailable)} are no longer available.");

        // Price is always computed on the server – never trust a client-supplied total.
        var price = show.TicketPrice;

        await using var tx = await bookings.BeginTransactionAsync();

        var booking = new Booking
        {
            BookingReference = Guid.NewGuid().ToString("N")[..20], // temporary, replaced below
            ShowId = r.ShowId,
            UserId = userId,
            CustomerName = r.CustomerName.Trim(),
            CustomerEmail = (string.IsNullOrWhiteSpace(r.CustomerEmail) ? accountEmail : r.CustomerEmail).Trim(),
            CustomerPhone = r.CustomerPhone.Trim(),
            BookingDate = DateTime.UtcNow,
            TotalAmount = price * showSeats.Count,
            Status = BookingStatus.Confirmed,
            BookingSeats = showSeats.Select(ss => new BookingSeat { SeatId = ss.SeatId, Seat = ss.Seat, Price = price }).ToList()
        };

        foreach (var ss in showSeats) ss.Status = SeatStatus.Booked;

        await bookings.AddAsync(booking);            // gets BookingId
        booking.BookingReference = $"MOV{booking.BookingId}";
        await bookings.SaveChangesAsync();

        await tx.CommitAsync();                       // disposing without commit rolls everything back
        return booking.ToResponse();
    }

    public async Task<List<BookingResponse>> GetAllAsync(int? showId, string? status)
    {
        BookingStatus? parsed = null;
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<BookingStatus>(status, true, out var s))
                throw new BadRequestException("status must be 'Confirmed' or 'Cancelled'.");
            parsed = s;
        }
        return (await bookings.GetAllAsync(showId, parsed)).Select(b => b.ToResponse()).ToList();
    }

    public async Task<List<BookingResponse>> GetMyBookingsAsync(string userId) =>
        (await bookings.GetByUserAsync(userId)).Select(b => b.ToResponse()).ToList();

    public async Task<BookingResponse> GetByIdAsync(int id, string userId, bool isAdmin)
    {
        var booking = await bookings.GetByIdAsync(id) ?? throw new NotFoundException($"Booking {id} was not found.");
        EnsureOwnerOrAdmin(booking, userId, isAdmin);
        return booking.ToResponse();
    }

    public async Task<List<BookingResponse>> GetByShowAsync(int showId)
    {
        _ = await shows.GetByIdAsync(showId) ?? throw new NotFoundException($"Show {showId} was not found.");
        return (await bookings.GetByShowAsync(showId)).Select(b => b.ToResponse()).ToList();
    }

    public async Task<BookingResponse> CancelAsync(int id, string userId, bool isAdmin)
    {
        var booking = await bookings.GetByIdAsync(id) ?? throw new NotFoundException($"Booking {id} was not found.");
        EnsureOwnerOrAdmin(booking, userId, isAdmin);

        if (booking.Status != BookingStatus.Confirmed)
            throw new ConflictException("Only a Confirmed booking can be cancelled.");

        var seatIds = booking.BookingSeats.Select(bs => bs.SeatId).ToList();
        var showSeats = await bookings.GetShowSeatsAsync(booking.ShowId, seatIds);

        await using var tx = await bookings.BeginTransactionAsync();
        booking.Status = BookingStatus.Cancelled;
        foreach (var ss in showSeats) ss.Status = SeatStatus.Available;
        await bookings.SaveChangesAsync();
        await tx.CommitAsync();

        // Booking and BookingSeats rows are preserved as history.
        return booking.ToResponse();
    }

    private static void EnsureOwnerOrAdmin(Booking booking, string userId, bool isAdmin)
    {
        if (!isAdmin && booking.UserId != userId)
            throw new ForbiddenException("You can only access your own bookings.");
    }
}
