using System.ComponentModel.DataAnnotations;

namespace MovieTicketBooking.DTOs.Theatres;

public class CreateTheatreRequest
{
    [Required, MaxLength(150)] public string Name { get; set; } = string.Empty;
    [Required, MaxLength(200)] public string Location { get; set; } = string.Empty;
}

public class UpdateTheatreRequest : CreateTheatreRequest { }

public class TheatreResponse
{
    public int TheatreId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
}
