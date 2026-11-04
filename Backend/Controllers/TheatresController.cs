using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieTicketBooking.DTOs.Theatres;
using MovieTicketBooking.Helpers;
using MovieTicketBooking.Services.Interfaces;

namespace MovieTicketBooking.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TheatresController(ITheatreService theatres) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<TheatreResponse>> Create(CreateTheatreRequest request)
    {
        var created = await theatres.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.TheatreId }, created);
    }

    [HttpGet]
    public async Task<ActionResult<List<TheatreResponse>>> GetAll(
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50) =>
        Ok(await theatres.GetAllAsync(pageNumber, pageSize));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TheatreResponse>> GetById(int id) => Ok(await theatres.GetByIdAsync(id));

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<TheatreResponse>> Update(int id, UpdateTheatreRequest request) =>
        Ok(await theatres.UpdateAsync(id, request));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        await theatres.DeleteAsync(id);
        return NoContent();
    }
}
