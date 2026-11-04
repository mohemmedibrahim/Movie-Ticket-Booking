namespace MovieTicketBooking.Exceptions;

public class NotFoundException(string message) : Exception(message);
public class ConflictException(string message) : Exception(message);
public class BadRequestException(string message) : Exception(message);
public class UnauthorizedException(string message = "Authentication failed.") : Exception(message);
public class ForbiddenException(string message = "You do not have permission to perform this action.") : Exception(message);
