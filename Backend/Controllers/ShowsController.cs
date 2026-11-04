using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieTicketBooking.DTOs;
using MovieTicketBooking.DTOs.Shows;
using MovieTicketBooking.Exceptions;
using MovieTicketBooking.Helpers;
using MovieTicketBooking.Services.Interfaces;

namespace MovieTicketBooking.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShowsController(IShowService shows) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ShowResponse>> Create(CreateShowRequest request)
    {
        var created = await shows.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.ShowId }, created);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<ShowResponse>>> GetAll(
        [FromQuery] DateOnly? date, [FromQuery] int? movieId, [FromQuery] int? theatreId,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10) =>
        Ok(await shows.GetAllAsync(pageNumber, pageSize, date, movieId, theatreId));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ShowResponse>> GetById(int id) => Ok(await shows.GetByIdAsync(id));

    [HttpGet("movie/{movieId:int}")]
    public async Task<ActionResult<List<ShowResponse>>> GetByMovie(int movieId) =>
        Ok(await shows.GetByMovieAsync(movieId));

    [HttpGet("theatre/{theatreId:int}")]
    public async Task<ActionResult<List<ShowResponse>>> GetByTheatre(int theatreId) =>
        Ok(await shows.GetByTheatreAsync(theatreId));

    [HttpGet("search")]
    public async Task<ActionResult<List<ShowResponse>>> SearchByDate([FromQuery] DateOnly? date) =>
        Ok(await shows.SearchByDateAsync(date ?? throw new BadRequestException("The 'date' query parameter (YYYY-MM-DD) is required.")));

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ShowResponse>> Update(int id, UpdateShowRequest request) =>
        Ok(await shows.UpdateAsync(id, request));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        await shows.DeleteAsync(id);
        return NoContent();
    }

    // ---- ShowSeats (public: customers need these to choose seats) ----

    [HttpGet("{showId:int}/seats")]
    public async Task<ActionResult<List<ShowSeatResponse>>> GetSeats(int showId) =>
        Ok(await shows.GetSeatsAsync(showId));

    [HttpGet("{showId:int}/seats/available")]
    public async Task<ActionResult<List<ShowSeatResponse>>> GetAvailableSeats(int showId) =>
        Ok(await shows.GetAvailableSeatsAsync(showId));

    [HttpGet("{showId:int}/seats/booked")]
    public async Task<ActionResult<List<ShowSeatResponse>>> GetBookedSeats(int showId) =>
        Ok(await shows.GetBookedSeatsAsync(showId));
}
