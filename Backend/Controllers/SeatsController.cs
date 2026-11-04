using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieTicketBooking.DTOs.Seats;
using MovieTicketBooking.Helpers;
using MovieTicketBooking.Services.Interfaces;

namespace MovieTicketBooking.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SeatsController(ISeatService seats) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<SeatResponse>> Create(CreateSeatRequest request)
    {
        var created = await seats.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.SeatId }, created);
    }

    [HttpPost("bulk")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<List<SeatResponse>>> CreateBulk(BulkCreateSeatsRequest request)
    {
        var created = await seats.CreateBulkAsync(request);
        return CreatedAtAction(nameof(GetByScreen), new { screenId = request.ScreenId }, created);
    }

    [HttpGet("screen/{screenId:int}")]
    public async Task<ActionResult<List<SeatResponse>>> GetByScreen(int screenId) =>
        Ok(await seats.GetByScreenAsync(screenId));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SeatResponse>> GetById(int id) => Ok(await seats.GetByIdAsync(id));

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<SeatResponse>> Update(int id, UpdateSeatRequest request) =>
        Ok(await seats.UpdateAsync(id, request));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        await seats.DeleteAsync(id);
        return NoContent();
    }
}
