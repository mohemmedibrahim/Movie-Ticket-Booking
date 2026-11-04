using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieTicketBooking.DTOs.Auth;
using MovieTicketBooking.Exceptions;
using MovieTicketBooking.Helpers;
using MovieTicketBooking.Services.Interfaces;

namespace MovieTicketBooking.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService auth) : ControllerBase
{
    /// <summary>Public self-registration. Always creates a Customer account.</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        var result = await auth.RegisterCustomerAsync(request);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request) =>
        Ok(await auth.LoginAsync(request));

    /// <summary>Admin only: create another Admin account.</summary>
    [HttpPost("register-admin")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<UserResponse>> RegisterAdmin(RegisterRequest request)
    {
        var result = await auth.RegisterAdminAsync(request);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>Current logged-in user.</summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserResponse>> Me() =>
        Ok(await auth.GetProfileAsync(User.GetUserId() ?? throw new UnauthorizedException()));
}
