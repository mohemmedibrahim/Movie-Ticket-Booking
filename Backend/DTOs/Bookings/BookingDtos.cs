using System.ComponentModel.DataAnnotations;

namespace MovieTicketBooking.DTOs.Bookings;

public class CreateBookingRequest
{
    [Required, MaxLength(100)] public string CustomerName { get; set; } = string.Empty;

    // Optional: defaults to the logged-in account's email.
    [EmailAddress, MaxLength(150)] public string? CustomerEmail { get; set; }

    [Required, RegularExpression(@"^\+?[0-9]{7,14}$", ErrorMessage = "Phone must be 7-14 digits, optionally starting with +.")]
    public string CustomerPhone { get; set; } = string.Empty;

    [Range(1, int.MaxValue)] public int ShowId { get; set; }
    [Required, MinLength(1)] public List<int> SeatIds { get; set; } = new();
}

public class BookingResponse
{
    public int BookingId { get; set; }
    public string BookingReference { get; set; } = string.Empty;
    public int ShowId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public DateTime BookingDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<string> Seats { get; set; } = new();
}
