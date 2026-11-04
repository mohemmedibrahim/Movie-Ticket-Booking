namespace MovieTicketBooking.Models;

public class Theatre
{
    public int TheatreId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;

    public ICollection<Screen> Screens { get; set; } = new List<Screen>();
}
