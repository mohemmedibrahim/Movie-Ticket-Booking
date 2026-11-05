import { Component, effect, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Api } from '../core/api.service';
import { Booking, Show } from '../core/models';
import { fmtDateTime, inr } from '../core/format';
import { Toasts, errorText } from '../core/toast.service';

@Component({
  selector: 'app-booking-detail',
  imports: [RouterLink],
  template: `
    @if (loading()) { <div class="spinner" role="status"></div> }
    @else if (error()) { <div class="empty">{{ error() }}</div> }
    @else if (b(); as b) {
      <div class="ticket" [class.cancelled]="b.status === 'Cancelled'">
        <div class="main stack">
          <div class="row spread">
            <span class="muted">Booking reference</span>
            <span class="badge" [class.ok]="b.status === 'Confirmed'" [class.bad]="b.status === 'Cancelled'">{{ b.status }}</span>
          </div>
          <h1 class="ref">{{ b.bookingReference }}</h1>
          @if (show(); as s) {
            <div><h2>{{ s.movieTitle }}</h2><p class="muted">{{ s.theatreName }} · {{ s.screenName }}</p><p>{{ when(s.startTime) }}</p></div>
          }
          <div class="chosen">@for (x of b.seats; track x) { <span class="badge seat">{{ x }}</span> }</div>
        </div>
        <div class="stub stack">
          <div><p class="muted">Name</p><b>{{ b.customerName }}</b></div>
          <div><p class="muted">Contact</p><b>{{ b.customerPhone }}</b><br><span class="muted">{{ b.customerEmail }}</span></div>
          <div><p class="muted">Total</p><b class="amt">{{ money(b.totalAmount) }}</b></div>
          <p class="hint">Booked {{ when(b.bookingDate) }}</p>
        </div>
      </div>
      <div class="row" style="margin-top:20px">
        <a routerLink="/my-bookings" class="btn">All my bookings</a>
        <a routerLink="/" class="btn ghost">Browse movies</a>
        @if (b.status === 'Confirmed') { <button class="btn danger" [disabled]="busy()" (click)="cancel()">Cancel booking</button> }
      </div>
    }
  `,
  styles: [`
    .ticket { display: grid; grid-template-columns: 1fr 280px; background: var(--surface); border: 1px solid var(--line); border-radius: 20px; overflow: hidden; }
    .main, .stub { padding: 28px; } .stub { background: var(--surface-2); border-left: 2px dashed var(--line); }
    .ref { font-size: clamp(28px, 5vw, 44px); color: var(--accent); letter-spacing: .02em; }
    .ticket.cancelled .ref { color: var(--muted); text-decoration: line-through; }
    .chosen { display: flex; gap: 8px; flex-wrap: wrap; } .badge.seat { font-size: 14px; padding: 5px 12px; color: var(--text); }
    .amt { font: 800 26px var(--font-display); } .stub p { margin: 0 0 2px; font-size: 13px; }
    @media (max-width: 700px) { .ticket { grid-template-columns: 1fr; } .stub { border-left: 0; border-top: 2px dashed var(--line); } }
  `],
})
export class BookingDetailPage {
  private api = inject(Api); private toast = inject(Toasts);
  id = input.required<string>();
  b = signal<Booking | null>(null); show = signal<Show | null>(null);
  loading = signal(true); error = signal(''); busy = signal(false);
  money = inr; when = fmtDateTime;

  constructor() {
    effect(() => {
      this.loading.set(true); this.error.set('');
      this.api.booking(+this.id()).subscribe({
        next: b => { this.b.set(b); this.loading.set(false); this.api.show(b.showId).subscribe({ next: s => this.show.set(s), error: () => {} }); },
        error: e => { this.error.set(e.status === 404 ? 'Booking not found.' : e.status === 403 ? 'This booking belongs to someone else.' : errorText(e)); this.loading.set(false); },
      });
    });
  }
  cancel() {
    if (!confirm('Cancel this booking? Your seats will be released.')) return;
    this.busy.set(true);
    this.api.cancelBooking(+this.id()).subscribe({
      next: b => { this.b.set(b); this.busy.set(false); this.toast.ok('Booking cancelled.'); },
      error: e => { this.busy.set(false); this.toast.err(errorText(e)); },
    });
  }
}
