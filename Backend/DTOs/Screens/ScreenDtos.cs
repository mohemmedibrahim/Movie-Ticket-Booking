using System.ComponentModel.DataAnnotations;

namespace MovieTicketBooking.DTOs.Screens;

public class CreateScreenRequest
{
    [Range(1, int.MaxValue)] public int TheatreId { get; set; }
    [Required, MaxLength(50)] public string ScreenName { get; set; } = string.Empty;
}

public class UpdateScreenRequest
{
    [Required, MaxLength(50)] public string ScreenName { get; set; } = string.Empty;
}

public class ScreenResponse
{
    public int ScreenId { get; set; }
    public int TheatreId { get; set; }
    public string TheatreName { get; set; } = string.Empty;
    public string ScreenName { get; set; } = string.Empty;
}
