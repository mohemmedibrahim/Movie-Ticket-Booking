namespace MovieTicketBooking.Models;

public class Screen
{
    public int ScreenId { get; set; }
    public int TheatreId { get; set; }
    public string ScreenName { get; set; } = string.Empty;

    public Theatre Theatre { get; set; } = null!;
    public ICollection<Seat> Seats { get; set; } = new List<Seat>();
    public ICollection<Show> Shows { get; set; } = new List<Show>();
}
