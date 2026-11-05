export interface PagedResponse<T> {
  items: T[]; pageNumber: number; pageSize: number; totalCount: number; totalPages: number;
}
export interface Movie {
  movieId: number; title: string; description?: string | null; durationMinutes: number;
  releaseDate: string; language: string; genre: string;
}
export type MovieInput = Omit<Movie, 'movieId'>;
export interface Theatre { theatreId: number; name: string; location: string; }
export type TheatreInput = Omit<Theatre, 'theatreId'>;
export interface Screen { screenId: number; theatreId: number; theatreName: string; screenName: string; }
export interface Seat { seatId: number; screenId: number; rowName: string; seatNumber: number; seatLabel: string; seatType: string; }
export interface BulkSeatRow { rowName: string; seatCount: number; seatType: 'Regular' | 'Premium'; }
export interface Show {
  showId: number; movieId: number; movieTitle: string; screenId: number; screenName: string;
  theatreId: number; theatreName: string; startTime: string; endTime: string; ticketPrice: number;
}
export interface ShowInput { movieId: number; screenId?: number; startTime: string; endTime: string; ticketPrice: number; }
export interface ShowSeat {
  seatId: number; rowName: string; seatNumber: number; seatLabel: string; seatType: string;
  price: number; status: 'Available' | 'Booked';
}
export interface Booking {
  bookingId: number; bookingReference: string; showId: number; customerName: string; customerEmail: string;
  customerPhone: string; bookingDate: string; totalAmount: number; status: 'Confirmed' | 'Cancelled'; seats: string[];
}
export interface CreateBooking { customerName: string; customerEmail?: string; customerPhone: string; showId: number; seatIds: number[]; }
export interface User { userId: string; fullName: string; email: string; phone?: string | null; roles: string[]; }
export interface AuthResponse { token: string; tokenType: string; expiresAtUtc: string; user: User; }
