namespace MovieTicketBooking.Helpers;

public static class Paging
{
    public const int MaxPageSize = 100;

    public static (int PageNumber, int PageSize) Normalize(int pageNumber, int pageSize) =>
        (Math.Max(1, pageNumber), Math.Clamp(pageSize, 1, MaxPageSize));
}
