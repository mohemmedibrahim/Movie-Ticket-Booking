using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MovieTicketBooking.Enums;
using MovieTicketBooking.Models;

namespace MovieTicketBooking.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<Theatre> Theatres => Set<Theatre>();
    public DbSet<Screen> Screens => Set<Screen>();
    public DbSet<Seat> Seats => Set<Seat>();
    public DbSet<Show> Shows => Set<Show>();
    public DbSet<ShowSeat> ShowSeats => Set<ShowSeat>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingSeat> BookingSeats => Set<BookingSeat>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b); // Identity tables

        b.Entity<ApplicationUser>(e =>
        {
            e.Property(u => u.FullName).IsRequired().HasMaxLength(100);
        });

        b.Entity<Movie>(e =>
        {
            e.ToTable("Movies", t => t.HasCheckConstraint("CK_Movies_DurationMinutes", "[DurationMinutes] > 0"));
            e.HasKey(x => x.MovieId);
            e.Property(x => x.Title).IsRequired().HasMaxLength(150);
            e.Property(x => x.Description).HasMaxLength(500);
            e.Property(x => x.ReleaseDate).HasColumnType("date");
            e.Property(x => x.Language).IsRequired().HasMaxLength(50);
            e.Property(x => x.Genre).IsRequired().HasMaxLength(50);
        });

        b.Entity<Theatre>(e =>
        {
            e.ToTable("Theatres");
            e.HasKey(x => x.TheatreId);
            e.Property(x => x.Name).IsRequired().HasMaxLength(150);
            e.Property(x => x.Location).IsRequired().HasMaxLength(200);
        });

        b.Entity<Screen>(e =>
        {
            e.ToTable("Screens");
            e.HasKey(x => x.ScreenId);
            e.Property(x => x.ScreenName).IsRequired().HasMaxLength(50);
            e.HasIndex(x => new { x.TheatreId, x.ScreenName }).IsUnique();
            e.HasOne(x => x.Theatre).WithMany(t => t.Screens)
                .HasForeignKey(x => x.TheatreId).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<Seat>(e =>
        {
            e.ToTable("Seats", t => t.HasCheckConstraint("CK_Seats_SeatNumber", "[SeatNumber] > 0"));
            e.HasKey(x => x.SeatId);
            e.Property(x => x.RowName).IsRequired().HasMaxLength(5);
            e.Property(x => x.SeatType).IsRequired().HasMaxLength(30);
            e.HasIndex(x => new { x.ScreenId, x.RowName, x.SeatNumber }).IsUnique();
            // Seats of a screen without any show history may be removed together with the screen.
            e.HasOne(x => x.Screen).WithMany(s => s.Seats)
                .HasForeignKey(x => x.ScreenId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Show>(e =>
        {
            e.ToTable("Shows", t =>
            {
                t.HasCheckConstraint("CK_Shows_EndAfterStart", "[EndTime] > [StartTime]");
                t.HasCheckConstraint("CK_Shows_TicketPrice", "[TicketPrice] >= 0");
            });
            e.HasKey(x => x.ShowId);
            e.Property(x => x.StartTime).HasColumnType("datetime2");
            e.Property(x => x.EndTime).HasColumnType("datetime2");
            e.Property(x => x.TicketPrice).HasPrecision(10, 2);
            e.HasOne(x => x.Movie).WithMany(m => m.Shows)
                .HasForeignKey(x => x.MovieId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Screen).WithMany(s => s.Shows)
                .HasForeignKey(x => x.ScreenId).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<ShowSeat>(e =>
        {
            e.ToTable("ShowSeats");
            e.HasKey(x => x.ShowSeatId);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            e.HasIndex(x => new { x.ShowId, x.SeatId }).IsUnique();
            e.HasOne(x => x.Show).WithMany(s => s.ShowSeats)
                .HasForeignKey(x => x.ShowId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Seat).WithMany(s => s.ShowSeats)
                .HasForeignKey(x => x.SeatId).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<Booking>(e =>
        {
            e.ToTable("Bookings", t => t.HasCheckConstraint("CK_Bookings_TotalAmount", "[TotalAmount] >= 0"));
            e.HasKey(x => x.BookingId);
            e.Property(x => x.BookingReference).IsRequired().HasMaxLength(30);
            e.HasIndex(x => x.BookingReference).IsUnique();
            e.Property(x => x.CustomerName).IsRequired().HasMaxLength(100);
            e.Property(x => x.CustomerEmail).IsRequired().HasMaxLength(150);
            e.Property(x => x.CustomerPhone).IsRequired().HasMaxLength(15);
            e.Property(x => x.BookingDate).HasColumnType("datetime2");
            e.Property(x => x.TotalAmount).HasPrecision(10, 2);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            e.HasOne(x => x.Show).WithMany(s => s.Bookings)
                .HasForeignKey(x => x.ShowId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.User).WithMany()
                .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<BookingSeat>(e =>
        {
            e.ToTable("BookingSeats", t => t.HasCheckConstraint("CK_BookingSeats_Price", "[Price] >= 0"));
            e.HasKey(x => x.BookingSeatId);
            e.Property(x => x.Price).HasPrecision(10, 2);
            e.HasOne(x => x.Booking).WithMany(bk => bk.BookingSeats)
                .HasForeignKey(x => x.BookingId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Seat).WithMany(s => s.BookingSeats)
                .HasForeignKey(x => x.SeatId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
