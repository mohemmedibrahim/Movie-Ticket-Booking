using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieTicketBooking.DTOs.Bookings;
using MovieTicketBooking.Exceptions;
using MovieTicketBooking.Helpers;
using MovieTicketBooking.Services.Interfaces;

namespace MovieTicketBooking.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // every booking endpoint requires a logged-in user; stricter rules are per action
public class BookingsController(IBookingService bookings) : ControllerBase
{
    private string UserId => User.GetUserId() ?? throw new UnauthorizedException();

    /// <summary>Create a booking for the logged-in user (Customer or Admin).</summary>
    [HttpPost]
    public async Task<ActionResult<BookingResponse>> Create(CreateBookingRequest request)
    {
        var created = await bookings.CreateAsync(request, UserId, User.GetEmail() ?? string.Empty);
        return CreatedAtAction(nameof(GetById), new { id = created.BookingId }, created);
    }

    /// <summary>Admin only: all bookings.</summary>
    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<List<BookingResponse>>> GetAll([FromQuery] int? showId, [FromQuery] string? status) =>
        Ok(await bookings.GetAllAsync(showId, status));

    /// <summary>The logged-in user's own bookings.</summary>
    [HttpGet("my")]
    public async Task<ActionResult<List<BookingResponse>>> GetMine() =>
        Ok(await bookings.GetMyBookingsAsync(UserId));

    /// <summary>Owner or Admin.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookingResponse>> GetById(int id) =>
        Ok(await bookings.GetByIdAsync(id, UserId, User.IsAdmin()));

    /// <summary>Admin only.</summary>
    [HttpGet("show/{showId:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<List<BookingResponse>>> GetByShow(int showId) =>
        Ok(await bookings.GetByShowAsync(showId));

    /// <summary>Owner or Admin.</summary>
    [HttpPut("{id:int}/cancel")]
    public async Task<ActionResult<BookingResponse>> Cancel(int id) =>
        Ok(await bookings.CancelAsync(id, UserId, User.IsAdmin()));
}
