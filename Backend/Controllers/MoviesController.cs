using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieTicketBooking.DTOs;
using MovieTicketBooking.DTOs.Movies;
using MovieTicketBooking.Helpers;
using MovieTicketBooking.Services.Interfaces;

namespace MovieTicketBooking.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MoviesController(IMovieService movies) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<MovieResponse>> Create(CreateMovieRequest request)
    {
        var created = await movies.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.MovieId }, created);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<MovieResponse>>> GetAll(
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? title = null) =>
        Ok(await movies.GetAllAsync(pageNumber, pageSize, title));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MovieResponse>> GetById(int id) => Ok(await movies.GetByIdAsync(id));

    [HttpGet("search")]
    public async Task<ActionResult<List<MovieResponse>>> Search([FromQuery] string title) =>
        Ok(await movies.SearchAsync(title));

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<MovieResponse>> Update(int id, UpdateMovieRequest request) =>
        Ok(await movies.UpdateAsync(id, request));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        await movies.DeleteAsync(id);
        return NoContent();
    }
}
