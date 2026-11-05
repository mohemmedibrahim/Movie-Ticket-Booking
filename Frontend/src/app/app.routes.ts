import { Routes } from '@angular/router';
import { adminGuard, authGuard } from './core/guards';

export const routes: Routes = [
  { path: '', title: 'Marquee · Now showing', loadComponent: () => import('./pages/movies').then(m => m.MoviesPage) },
  { path: 'movies/:id', title: 'Movie · Marquee', loadComponent: () => import('./pages/movie-detail').then(m => m.MovieDetailPage) },
  { path: 'shows/:id/seats', title: 'Choose seats · Marquee', canActivate: [authGuard], loadComponent: () => import('./pages/seat-select').then(m => m.SeatSelectPage) },
  { path: 'my-bookings', title: 'My bookings · Marquee', canActivate: [authGuard], loadComponent: () => import('./pages/my-bookings').then(m => m.MyBookingsPage) },
  { path: 'bookings/:id', title: 'Booking · Marquee', canActivate: [authGuard], loadComponent: () => import('./pages/booking-detail').then(m => m.BookingDetailPage) },
  { path: 'login', title: 'Sign in · Marquee', loadComponent: () => import('./pages/auth').then(m => m.LoginPage) },
  { path: 'register', title: 'Create account · Marquee', loadComponent: () => import('./pages/auth').then(m => m.RegisterPage) },
  {
    path: 'admin', canActivate: [adminGuard], loadComponent: () => import('./pages/admin/admin-shell').then(m => m.AdminShell),
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'movies' },
      { path: 'movies', title: 'Admin · Movies', loadComponent: () => import('./pages/admin/admin-movies').then(m => m.AdminMovies) },
      { path: 'theatres', title: 'Admin · Theatres', loadComponent: () => import('./pages/admin/admin-theatres').then(m => m.AdminTheatres) },
      { path: 'screens', title: 'Admin · Screens & seats', loadComponent: () => import('./pages/admin/admin-screens').then(m => m.AdminScreens) },
      { path: 'shows', title: 'Admin · Shows', loadComponent: () => import('./pages/admin/admin-shows').then(m => m.AdminShows) },
      { path: 'bookings', title: 'Admin · Bookings', loadComponent: () => import('./pages/admin/admin-bookings').then(m => m.AdminBookings) },
      { path: 'users', title: 'Admin · Add admin', loadComponent: () => import('./pages/admin/admin-users').then(m => m.AdminUsers) },
    ],
  },
  { path: '**', redirectTo: '' },
];
