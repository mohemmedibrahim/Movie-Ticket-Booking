namespace MovieTicketBooking.Models;

public class Show
{
    public int ShowId { get; set; }
    public int MovieId { get; set; }
    public int ScreenId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal TicketPrice { get; set; }

    public Movie Movie { get; set; } = null!;
    public Screen Screen { get; set; } = null!;
    public ICollection<ShowSeat> ShowSeats { get; set; } = new List<ShowSeat>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
