using System.ComponentModel.DataAnnotations;

namespace MovieTicketBooking.DTOs.Seats;

public class CreateSeatRequest
{
    [Range(1, int.MaxValue)] public int ScreenId { get; set; }
    [Required, MaxLength(5)] public string RowName { get; set; } = string.Empty;
    [Range(1, int.MaxValue, ErrorMessage = "SeatNumber must be greater than zero.")]
    public int SeatNumber { get; set; }
    [Required, AllowedValues("Regular", "Premium")] public string SeatType { get; set; } = "Regular";
}

public class UpdateSeatRequest
{
    [Required, MaxLength(5)] public string RowName { get; set; } = string.Empty;
    [Range(1, int.MaxValue, ErrorMessage = "SeatNumber must be greater than zero.")]
    public int SeatNumber { get; set; }
    [Required, AllowedValues("Regular", "Premium")] public string SeatType { get; set; } = "Regular";
}

public class BulkSeatRow
{
    [Required, MaxLength(5)] public string RowName { get; set; } = string.Empty;
    [Range(1, 100)] public int SeatCount { get; set; }
    [Required, AllowedValues("Regular", "Premium")] public string SeatType { get; set; } = "Regular";
}

public class BulkCreateSeatsRequest
{
    [Range(1, int.MaxValue)] public int ScreenId { get; set; }
    [Required, MinLength(1)] public List<BulkSeatRow> Rows { get; set; } = new();
}

public class SeatResponse
{
    public int SeatId { get; set; }
    public int ScreenId { get; set; }
    public string RowName { get; set; } = string.Empty;
    public int SeatNumber { get; set; }
    public string SeatLabel { get; set; } = string.Empty;
    public string SeatType { get; set; } = string.Empty;
}
