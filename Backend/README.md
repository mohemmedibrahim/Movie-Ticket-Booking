# Movie Ticket Booking System – V1 + Authentication & Authorization

ASP.NET Core Web API · .NET 10 · EF Core 10 · SQL Server · Controller → Service → Repository → DbContext

Everything from the original implementation guide is unchanged (8 tables, all API routes, workflows,
validation, exception handling). Authentication/authorization was **added on top**.

## Run it

Prerequisites: .NET 10 SDK, SQL Server (LocalDB, Express, or Docker).

1. Check `ConnectionStrings:DefaultConnection` in `appsettings.json`.
   - Windows LocalDB (default): works out of the box.
   - Docker / Linux / macOS:
     `docker run -e ACCEPT_EULA=Y -e MSSQL_SA_PASSWORD='Your_strong_Pass123' -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest`
     then use `Server=localhost,1433;Database=MovieTicketBookingDB;User Id=sa;Password=Your_strong_Pass123;TrustServerCertificate=True`
2. (Recommended) create the migration once:
   ```
   dotnet tool install --global dotnet-ef
   dotnet ef migrations add InitialCreate
   ```
   If you skip this, the app creates the schema with `EnsureCreated()` on first run (fine for trying it out).
3. `dotnet run` (the app applies migrations and seeds roles + the admin account at startup).
4. Open **https://localhost:7100/scalar/v1**. Use `MovieTicketBooking.http` for ready-made requests.

## Default admin (change it!)
`admin@movieticket.local` / `Admin@12345` – configured under `SeedAdmin` in `appsettings.json`.

## Authentication
- JWT bearer tokens via ASP.NET Core Identity (`ApplicationUser`, roles `Admin` and `Customer`).
- `POST /api/auth/register` – public, always creates a **Customer**; returns a token.
- `POST /api/auth/login` – returns a token (5 failed attempts → 10 minute lockout).
- `GET /api/auth/me` – current user.
- `POST /api/auth/register-admin` – **Admin only**.
- Send `Authorization: Bearer <token>` on protected calls. In Scalar, use the Authorize box.

## Authorization matrix
| Endpoints | Access |
|---|---|
| GET movies / theatres / screens / seats / shows / show seats | Public |
| POST / PUT / DELETE movies, theatres, screens, seats, shows | Admin |
| POST /api/bookings | Any logged-in user (identity & default email come from the token) |
| GET /api/bookings/my | Logged-in user (own bookings) |
| GET /api/bookings/{id}, PUT /api/bookings/{id}/cancel | Booking owner or Admin |
| GET /api/bookings, GET /api/bookings/show/{showId} | Admin |

Error responses: 401 (no/invalid token), 403 (wrong role / not your booking), plus 400/404/409 as in the guide.

## What changed vs. the original guide
- `Booking.UserId` (FK → AspNetUsers) added; no other table columns changed.
- `ApplicationDbContext` now derives from `IdentityDbContext<ApplicationUser>` (adds the `AspNet*` tables).
- New files: `AuthController`, `AuthService`, `TokenService`, `ApplicationUser`, `IdentitySeeder`, `JwtSettings`, auth DTOs.
- `POST /api/bookings`: `customerEmail` is optional (defaults to the account email); `customerName` and `customerPhone` still required.
- `PUT /api/shows/{id}` does not accept `screenId` (a show's seats are tied to its screen).
- Seats cannot be added to a screen once it has shows (ShowSeats are generated only at show creation).

## Known limitations (unchanged from V1)
- Booking availability check + transaction is **not** race-condition safe under simultaneous requests (reserved for V2).
- No payment, no seat holds.
- Before any real deployment: set `Jwt:Key` via user-secrets/environment variables, change the seed admin password, restrict CORS.
