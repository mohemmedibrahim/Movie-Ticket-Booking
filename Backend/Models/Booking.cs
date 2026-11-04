using MovieTicketBooking.Enums;

namespace MovieTicketBooking.Models;

public class Booking
{
    public int BookingId { get; set; }
    public string BookingReference { get; set; } = string.Empty;
    public int ShowId { get; set; }

    // Added with authentication: the account that made the booking.
    public string UserId { get; set; } = string.Empty;

    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public DateTime BookingDate { get; set; }
    public decimal TotalAmount { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Confirmed;

    public Show Show { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
    public ICollection<BookingSeat> BookingSeats { get; set; } = new List<BookingSeat>();
}
