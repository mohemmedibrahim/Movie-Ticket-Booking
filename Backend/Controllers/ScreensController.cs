using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieTicketBooking.DTOs.Screens;
using MovieTicketBooking.Helpers;
using MovieTicketBooking.Services.Interfaces;

namespace MovieTicketBooking.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScreensController(IScreenService screens) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ScreenResponse>> Create(CreateScreenRequest request)
    {
        var created = await screens.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.ScreenId }, created);
    }

    [HttpGet]
    public async Task<ActionResult<List<ScreenResponse>>> GetAll([FromQuery] int? theatreId) =>
        Ok(await screens.GetAllAsync(theatreId));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ScreenResponse>> GetById(int id) => Ok(await screens.GetByIdAsync(id));

    [HttpGet("theatre/{theatreId:int}")]
    public async Task<ActionResult<List<ScreenResponse>>> GetByTheatre(int theatreId) =>
        Ok(await screens.GetByTheatreAsync(theatreId));

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ScreenResponse>> Update(int id, UpdateScreenRequest request) =>
        Ok(await screens.UpdateAsync(id, request));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        await screens.DeleteAsync(id);
        return NoContent();
    }
}
