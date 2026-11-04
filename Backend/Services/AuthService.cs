using Microsoft.AspNetCore.Identity;
using MovieTicketBooking.DTOs.Auth;
using MovieTicketBooking.Exceptions;
using MovieTicketBooking.Helpers;
using MovieTicketBooking.Models;
using MovieTicketBooking.Services.Interfaces;

namespace MovieTicketBooking.Services;

public class AuthService(UserManager<ApplicationUser> users, ITokenService tokens) : IAuthService
{
    public async Task<AuthResponse> RegisterCustomerAsync(RegisterRequest r)
    {
        var user = await CreateUserAsync(r, Roles.Customer);
        return await BuildAuthResponseAsync(user);
    }

    public async Task<UserResponse> RegisterAdminAsync(RegisterRequest r)
    {
        var user = await CreateUserAsync(r, Roles.Admin);
        return await ToUserResponseAsync(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest r)
    {
        var user = await users.FindByEmailAsync(r.Email);
        // Same message for unknown email / wrong password so accounts cannot be enumerated.
        if (user is null) throw new UnauthorizedException("Invalid email or password.");

        if (await users.IsLockedOutAsync(user))
            throw new UnauthorizedException("Account is temporarily locked after too many failed attempts. Try again later.");

        if (!await users.CheckPasswordAsync(user, r.Password))
        {
            await users.AccessFailedAsync(user);
            throw new UnauthorizedException("Invalid email or password.");
        }

        await users.ResetAccessFailedCountAsync(user);
        return await BuildAuthResponseAsync(user);
    }

    public async Task<UserResponse> GetProfileAsync(string userId)
    {
        var user = await users.FindByIdAsync(userId) ?? throw new NotFoundException("User not found.");
        return await ToUserResponseAsync(user);
    }

    private async Task<ApplicationUser> CreateUserAsync(RegisterRequest r, string role)
    {
        var email = r.Email.Trim();
        if (await users.FindByEmailAsync(email) is not null)
            throw new ConflictException("An account with this email already exists.");

        var user = new ApplicationUser
        {
            UserName = email, Email = email, FullName = r.FullName.Trim(), PhoneNumber = r.Phone.Trim()
        };

        var create = await users.CreateAsync(user, r.Password);
        if (!create.Succeeded)
            throw new BadRequestException(string.Join(" ", create.Errors.Select(e => e.Description)));

        var addRole = await users.AddToRoleAsync(user, role);
        if (!addRole.Succeeded)
        {
            await users.DeleteAsync(user); // do not leave a role-less account behind
            throw new BadRequestException("Could not assign role to the new account.");
        }
        return user;
    }

    private async Task<AuthResponse> BuildAuthResponseAsync(ApplicationUser user)
    {
        var roles = await users.GetRolesAsync(user);
        var (token, expires) = tokens.CreateToken(user, roles);
        return new AuthResponse { Token = token, ExpiresAtUtc = expires, User = await ToUserResponseAsync(user) };
    }

    private async Task<UserResponse> ToUserResponseAsync(ApplicationUser user) => new()
    {
        UserId = user.Id, FullName = user.FullName, Email = user.Email ?? string.Empty,
        Phone = user.PhoneNumber, Roles = await users.GetRolesAsync(user)
    };
}
