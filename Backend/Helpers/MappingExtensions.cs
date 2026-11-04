using MovieTicketBooking.DTOs.Bookings;
using MovieTicketBooking.DTOs.Movies;
using MovieTicketBooking.DTOs.Screens;
using MovieTicketBooking.DTOs.Seats;
using MovieTicketBooking.DTOs.Shows;
using MovieTicketBooking.DTOs.Theatres;
using MovieTicketBooking.Models;

namespace MovieTicketBooking.Helpers;

public static class MappingExtensions
{
    public static MovieResponse ToResponse(this Movie m) => new()
    {
        MovieId = m.MovieId, Title = m.Title, Description = m.Description,
        DurationMinutes = m.DurationMinutes, ReleaseDate = m.ReleaseDate,
        Language = m.Language, Genre = m.Genre
    };

    public static TheatreResponse ToResponse(this Theatre t) => new()
    {
        TheatreId = t.TheatreId, Name = t.Name, Location = t.Location
    };

    public static ScreenResponse ToResponse(this Screen s) => new()
    {
        ScreenId = s.ScreenId, TheatreId = s.TheatreId,
        TheatreName = s.Theatre?.Name ?? string.Empty, ScreenName = s.ScreenName
    };

    public static SeatResponse ToResponse(this Seat s) => new()
    {
        SeatId = s.SeatId, ScreenId = s.ScreenId, RowName = s.RowName,
        SeatNumber = s.SeatNumber, SeatLabel = $"{s.RowName}{s.SeatNumber}", SeatType = s.SeatType
    };

    public static ShowResponse ToResponse(this Show s) => new()
    {
        ShowId = s.ShowId, MovieId = s.MovieId, MovieTitle = s.Movie?.Title ?? string.Empty,
        ScreenId = s.ScreenId, ScreenName = s.Screen?.ScreenName ?? string.Empty,
        TheatreId = s.Screen?.TheatreId ?? 0, TheatreName = s.Screen?.Theatre?.Name ?? string.Empty,
        StartTime = s.StartTime, EndTime = s.EndTime, TicketPrice = s.TicketPrice
    };

    public static ShowSeatResponse ToResponse(this ShowSeat ss, decimal ticketPrice) => new()
    {
        SeatId = ss.SeatId, RowName = ss.Seat.RowName, SeatNumber = ss.Seat.SeatNumber,
        SeatLabel = $"{ss.Seat.RowName}{ss.Seat.SeatNumber}", SeatType = ss.Seat.SeatType,
        Price = ticketPrice, Status = ss.Status.ToString()
    };

    public static BookingResponse ToResponse(this Booking b) => new()
    {
        BookingId = b.BookingId, BookingReference = b.BookingReference, ShowId = b.ShowId,
        CustomerName = b.CustomerName, CustomerEmail = b.CustomerEmail, CustomerPhone = b.CustomerPhone,
        BookingDate = b.BookingDate, TotalAmount = b.TotalAmount, Status = b.Status.ToString(),
        Seats = b.BookingSeats
            .OrderBy(x => x.Seat.RowName).ThenBy(x => x.Seat.SeatNumber)
            .Select(x => $"{x.Seat.RowName}{x.Seat.SeatNumber}").ToList()
    };
}
