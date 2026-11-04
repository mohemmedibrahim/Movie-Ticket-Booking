using MovieTicketBooking.DTOs;
using MovieTicketBooking.DTOs.Movies;
using MovieTicketBooking.Exceptions;
using MovieTicketBooking.Helpers;
using MovieTicketBooking.Models;
using MovieTicketBooking.Repositories.Interfaces;
using MovieTicketBooking.Services.Interfaces;

namespace MovieTicketBooking.Services;

public class MovieService(IMovieRepository movies) : IMovieService
{
    public async Task<MovieResponse> CreateAsync(CreateMovieRequest r)
    {
        var movie = new Movie
        {
            Title = r.Title.Trim(), Description = r.Description?.Trim(),
            DurationMinutes = r.DurationMinutes, ReleaseDate = r.ReleaseDate!.Value,
            Language = r.Language.Trim(), Genre = r.Genre.Trim()
        };
        await movies.AddAsync(movie);
        return movie.ToResponse();
    }

    public async Task<PagedResponse<MovieResponse>> GetAllAsync(int pageNumber, int pageSize, string? title)
    {
        (pageNumber, pageSize) = Paging.Normalize(pageNumber, pageSize);
        var (items, total) = await movies.GetPagedAsync(pageNumber, pageSize, title);
        return new PagedResponse<MovieResponse>
        {
            Items = items.Select(m => m.ToResponse()).ToList(),
            PageNumber = pageNumber, PageSize = pageSize, TotalCount = total
        };
    }

    public async Task<MovieResponse> GetByIdAsync(int id) =>
        (await movies.GetByIdAsync(id) ?? throw new NotFoundException($"Movie {id} was not found.")).ToResponse();

    public async Task<MovieResponse> UpdateAsync(int id, UpdateMovieRequest r)
    {
        var movie = await movies.GetByIdAsync(id) ?? throw new NotFoundException($"Movie {id} was not found.");
        movie.Title = r.Title.Trim();
        movie.Description = r.Description?.Trim();
        movie.DurationMinutes = r.DurationMinutes;
        movie.ReleaseDate = r.ReleaseDate!.Value;
        movie.Language = r.Language.Trim();
        movie.Genre = r.Genre.Trim();
        await movies.UpdateAsync(movie);
        return movie.ToResponse();
    }

    public async Task DeleteAsync(int id)
    {
        var movie = await movies.GetByIdAsync(id) ?? throw new NotFoundException($"Movie {id} was not found.");
        if (await movies.HasShowsAsync(id))
            throw new ConflictException("This movie has scheduled shows and cannot be deleted.");
        await movies.DeleteAsync(movie);
    }

    public async Task<List<MovieResponse>> SearchAsync(string title)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new BadRequestException("The 'title' query parameter is required.");
        return (await movies.SearchByTitleAsync(title.Trim())).Select(m => m.ToResponse()).ToList();
    }
}
