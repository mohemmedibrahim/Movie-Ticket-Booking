using System.ComponentModel.DataAnnotations;

namespace MovieTicketBooking.DTOs.Movies;

public class CreateMovieRequest
{
    [Required, MaxLength(150)] public string Title { get; set; } = string.Empty;
    [MaxLength(500)] public string? Description { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "DurationMinutes must be greater than zero.")]
    public int DurationMinutes { get; set; }
    [Required] public DateOnly? ReleaseDate { get; set; }
    [Required, MaxLength(50)] public string Language { get; set; } = string.Empty;
    [Required, MaxLength(50)] public string Genre { get; set; } = string.Empty;
}

public class UpdateMovieRequest : CreateMovieRequest { }

public class MovieResponse
{
    public int MovieId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DurationMinutes { get; set; }
    public DateOnly ReleaseDate { get; set; }
    public string Language { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
}
