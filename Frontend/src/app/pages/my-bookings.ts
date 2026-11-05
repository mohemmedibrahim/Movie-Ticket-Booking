import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Api } from '../core/api.service';
import { Booking } from '../core/models';
import { fmtDateTime, inr } from '../core/format';
import { Toasts, errorText } from '../core/toast.service';

@Component({
  selector: 'app-my-bookings',
  imports: [RouterLink],
  template: `
    <h1 style="margin-bottom:24px">My bookings</h1>
    @if (loading()) { <div class="spinner" role="status"></div> }
    @else if (error()) { <div class="empty">{{ error() }}</div> }
    @else if (!items().length) { <div class="empty">You haven't booked anything yet.<br><br><a routerLink="/" class="btn primary">Browse movies</a></div> }
    @else {
      <div class="stack">
        @for (b of items(); track b.bookingId) {
          <div class="card row spread">
            <a [routerLink]="['/bookings', b.bookingId]" class="grow stack" style="gap:6px">
              <div class="row"><b class="ref">{{ b.bookingReference }}</b>
                <span class="badge" [class.ok]="b.status === 'Confirmed'" [class.bad]="b.status === 'Cancelled'">{{ b.status }}</span></div>
              <p class="muted">Seats {{ b.seats.join(', ') }} · {{ money(b.totalAmount) }} · booked {{ when(b.bookingDate) }}</p>
            </a>
            @if (b.status === 'Confirmed') { <button class="btn sm danger" (click)="cancel(b)">Cancel</button> }
          </div>
        }
      </div>
    }
  `,
  styles: [`.ref { font: 700 18px var(--font-display); color: var(--accent); }`],
})
export class MyBookingsPage {
  private api = inject(Api); private toast = inject(Toasts);
  items = signal<Booking[]>([]); loading = signal(true); error = signal('');
  money = inr; when = fmtDateTime;
  constructor() {
    this.api.myBookings().subscribe({
      next: r => { this.items.set(r); this.loading.set(false); },
      error: e => { this.error.set(errorText(e)); this.loading.set(false); },
    });
  }
  cancel(b: Booking) {
    if (!confirm(`Cancel booking ${b.bookingReference}?`)) return;
    this.api.cancelBooking(b.bookingId).subscribe({
      next: u => { this.items.update(l => l.map(x => x.bookingId === u.bookingId ? u : x)); this.toast.ok('Booking cancelled.'); },
      error: e => this.toast.err(errorText(e)),
    });
  }
}
