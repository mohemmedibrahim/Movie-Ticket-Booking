using System.Security.Claims;

namespace MovieTicketBooking.Helpers;

public static class ClaimsPrincipalExtensions
{
    // JWT uses short claim names ("sub", "email") – see Program.cs (MapInboundClaims = false).
    public static string? GetUserId(this ClaimsPrincipal user) => user.FindFirst("sub")?.Value;
    public static string? GetEmail(this ClaimsPrincipal user) => user.FindFirst("email")?.Value;
    public static bool IsAdmin(this ClaimsPrincipal user) => user.IsInRole(Roles.Admin);
}
