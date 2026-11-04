using System.ComponentModel.DataAnnotations;

namespace MovieTicketBooking.DTOs.Shows;

public class CreateShowRequest : IValidatableObject
{
    [Range(1, int.MaxValue)] public int MovieId { get; set; }
    [Range(1, int.MaxValue)] public int ScreenId { get; set; }
    [Required] public DateTime? StartTime { get; set; }
    [Required] public DateTime? EndTime { get; set; }
    [Range(0, 100000)] public decimal TicketPrice { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext ctx)
    {
        if (StartTime.HasValue && EndTime.HasValue && EndTime <= StartTime)
            yield return new ValidationResult("EndTime must be later than StartTime.", new[] { nameof(EndTime) });
    }
}

// The screen of an existing show cannot change (its ShowSeats are tied to that screen).
public class UpdateShowRequest : IValidatableObject
{
    [Range(1, int.MaxValue)] public int MovieId { get; set; }
    [Required] public DateTime? StartTime { get; set; }
    [Required] public DateTime? EndTime { get; set; }
    [Range(0, 100000)] public decimal TicketPrice { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext ctx)
    {
        if (StartTime.HasValue && EndTime.HasValue && EndTime <= StartTime)
            yield return new ValidationResult("EndTime must be later than StartTime.", new[] { nameof(EndTime) });
    }
}

public class ShowResponse
{
    public int ShowId { get; set; }
    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public int ScreenId { get; set; }
    public string ScreenName { get; set; } = string.Empty;
    public int TheatreId { get; set; }
    public string TheatreName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal TicketPrice { get; set; }
}

public class ShowSeatResponse
{
    public int SeatId { get; set; }
    public string RowName { get; set; } = string.Empty;
    public int SeatNumber { get; set; }
    public string SeatLabel { get; set; } = string.Empty;
    public string SeatType { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Status { get; set; } = string.Empty;
}
