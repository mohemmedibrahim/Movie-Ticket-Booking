namespace MovieTicketBooking.Models;

public class Movie
{
    public int MovieId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DurationMinutes { get; set; }
    public DateOnly ReleaseDate { get; set; }
    public string Language { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;

    public ICollection<Show> Shows { get; set; } = new List<Show>();
}
