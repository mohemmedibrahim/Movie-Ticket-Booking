import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import * as M from './models';

@Injectable({ providedIn: 'root' })
export class Api {
  private http = inject(HttpClient);
  private base = environment.apiUrl;

  // ---- auth ----
  login(email: string, password: string) { return this.http.post<M.AuthResponse>(`${this.base}/auth/login`, { email, password }); }
  register(b: { fullName: string; email: string; phone: string; password: string }) { return this.http.post<M.AuthResponse>(`${this.base}/auth/register`, b); }
  registerAdmin(b: { fullName: string; email: string; phone: string; password: string }) { return this.http.post<M.User>(`${this.base}/auth/register-admin`, b); }
  me() { return this.http.get<M.User>(`${this.base}/auth/me`); }

  // ---- movies ----
  movies(pageNumber = 1, pageSize = 100, title?: string) {
    let p = new HttpParams().set('pageNumber', pageNumber).set('pageSize', pageSize);
    if (title) p = p.set('title', title);
    return this.http.get<M.PagedResponse<M.Movie>>(`${this.base}/movies`, { params: p });
  }
  movie(id: number) { return this.http.get<M.Movie>(`${this.base}/movies/${id}`); }
  createMovie(b: M.MovieInput) { return this.http.post<M.Movie>(`${this.base}/movies`, b); }
  updateMovie(id: number, b: M.MovieInput) { return this.http.put<M.Movie>(`${this.base}/movies/${id}`, b); }
  deleteMovie(id: number) { return this.http.delete<void>(`${this.base}/movies/${id}`); }

  // ---- theatres ----
  theatres() { return this.http.get<M.Theatre[]>(`${this.base}/theatres`, { params: { pageNumber: 1, pageSize: 100 } }); }
  createTheatre(b: M.TheatreInput) { return this.http.post<M.Theatre>(`${this.base}/theatres`, b); }
  updateTheatre(id: number, b: M.TheatreInput) { return this.http.put<M.Theatre>(`${this.base}/theatres/${id}`, b); }
  deleteTheatre(id: number) { return this.http.delete<void>(`${this.base}/theatres/${id}`); }

  // ---- screens & seats ----
  screens(theatreId?: number) {
    let p = new HttpParams(); if (theatreId) p = p.set('theatreId', theatreId);
    return this.http.get<M.Screen[]>(`${this.base}/screens`, { params: p });
  }
  createScreen(b: { theatreId: number; screenName: string }) { return this.http.post<M.Screen>(`${this.base}/screens`, b); }
  updateScreen(id: number, b: { screenName: string }) { return this.http.put<M.Screen>(`${this.base}/screens/${id}`, b); }
  deleteScreen(id: number) { return this.http.delete<void>(`${this.base}/screens/${id}`); }
  seatsByScreen(screenId: number) { return this.http.get<M.Seat[]>(`${this.base}/seats/screen/${screenId}`); }
  bulkSeats(b: { screenId: number; rows: M.BulkSeatRow[] }) { return this.http.post<M.Seat[]>(`${this.base}/seats/bulk`, b); }
  deleteSeat(id: number) { return this.http.delete<void>(`${this.base}/seats/${id}`); }

  // ---- shows ----
  shows(f: { date?: string; movieId?: number; theatreId?: number; pageNumber?: number; pageSize?: number } = {}) {
    let p = new HttpParams().set('pageNumber', f.pageNumber ?? 1).set('pageSize', f.pageSize ?? 100);
    if (f.date) p = p.set('date', f.date);
    if (f.movieId) p = p.set('movieId', f.movieId);
    if (f.theatreId) p = p.set('theatreId', f.theatreId);
    return this.http.get<M.PagedResponse<M.Show>>(`${this.base}/shows`, { params: p });
  }
  show(id: number) { return this.http.get<M.Show>(`${this.base}/shows/${id}`); }
  showsByMovie(movieId: number) { return this.http.get<M.Show[]>(`${this.base}/shows/movie/${movieId}`); }
  showSeats(showId: number) { return this.http.get<M.ShowSeat[]>(`${this.base}/shows/${showId}/seats`); }
  createShow(b: M.ShowInput) { return this.http.post<M.Show>(`${this.base}/shows`, b); }
  updateShow(id: number, b: M.ShowInput) { return this.http.put<M.Show>(`${this.base}/shows/${id}`, b); }
  deleteShow(id: number) { return this.http.delete<void>(`${this.base}/shows/${id}`); }

  // ---- bookings ----
  createBooking(b: M.CreateBooking): Observable<M.Booking> { return this.http.post<M.Booking>(`${this.base}/bookings`, b); }
  myBookings() { return this.http.get<M.Booking[]>(`${this.base}/bookings/my`); }
  booking(id: number) { return this.http.get<M.Booking>(`${this.base}/bookings/${id}`); }
  cancelBooking(id: number) { return this.http.put<M.Booking>(`${this.base}/bookings/${id}/cancel`, {}); }
  allBookings(showId?: number, status?: string) {
    let p = new HttpParams(); if (showId) p = p.set('showId', showId); if (status) p = p.set('status', status);
    return this.http.get<M.Booking[]>(`${this.base}/bookings`, { params: p });
  }
}
