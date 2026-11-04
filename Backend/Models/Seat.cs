namespace MovieTicketBooking.Models;

public class Seat
{
    public int SeatId { get; set; }
    public int ScreenId { get; set; }
    public string RowName { get; set; } = string.Empty;
    public int SeatNumber { get; set; }
    public string SeatType { get; set; } = string.Empty;

    public Screen Screen { get; set; } = null!;
    public ICollection<ShowSeat> ShowSeats { get; set; } = new List<ShowSeat>();
    public ICollection<BookingSeat> BookingSeats { get; set; } = new List<BookingSeat>();
}
