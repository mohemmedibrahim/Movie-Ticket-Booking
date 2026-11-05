import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Api } from '../../core/api.service';
import { Booking } from '../../core/models';
import { fmtDateTime, inr } from '../../core/format';
import { Toasts, errorText } from '../../core/toast.service';

@Component({
  selector: 'app-admin-bookings',
  imports: [FormsModule],
  template: `
    <div class="row spread" style="margin-bottom:16px"><h2>All bookings</h2></div>
    <div class="row" style="margin-bottom:16px">
      <label class="f">Status<select [ngModel]="status()" (ngModelChange)="status.set($event); load()"><option value="">All</option><option>Confirmed</option><option>Cancelled</option></select></label>
      <label class="f">Show ID<input type="number" min="1" style="width:120px" [ngModel]="showId()" (ngModelChange)="showId.set($event)" (change)="load()"></label>
    </div>
    @if (loading()) { <div class="spinner"></div> }
    @else if (!items().length) { <div class="empty">No bookings found.</div> }
    @else {
      <div class="tbl-wrap"><table>
        <thead><tr><th>Ref</th><th>Customer</th><th>Show</th><th>Seats</th><th>Total</th><th>Status</th><th>Booked</th><th></th></tr></thead>
        <tbody>@for (b of items(); track b.bookingId) {
          <tr><td><b>{{ b.bookingReference }}</b></td><td>{{ b.customerName }}<br><span class="muted">{{ b.customerEmail }}</span></td><td>#{{ b.showId }}</td>
            <td>{{ b.seats.join(', ') }}</td><td>{{ money(b.totalAmount) }}</td>
            <td><span class="badge" [class.ok]="b.status === 'Confirmed'" [class.bad]="b.status === 'Cancelled'">{{ b.status }}</span></td><td>{{ when(b.bookingDate) }}</td>
            <td class="actions">@if (b.status === 'Confirmed') { <button class="btn sm danger" (click)="cancel(b)">Cancel</button> }</td></tr>
        }</tbody></table></div>
    }
  `,
})
export class AdminBookings {
  private api = inject(Api); private toast = inject(Toasts);
  items = signal<Booking[]>([]); loading = signal(true); status = signal(''); showId = signal<number | null>(null);
  money = inr; when = fmtDateTime;
  constructor() { this.load(); }
  load() {
    this.loading.set(true);
    this.api.allBookings(this.showId() || undefined, this.status() || undefined).subscribe({
      next: r => { this.items.set(r); this.loading.set(false); }, error: e => { this.toast.err(errorText(e)); this.loading.set(false); } });
  }
  cancel(b: Booking) {
    if (!confirm(`Cancel booking ${b.bookingReference}?`)) return;
    this.api.cancelBooking(b.bookingId).subscribe({ next: () => { this.toast.ok('Cancelled.'); this.load(); }, error: e => this.toast.err(errorText(e)) });
  }
}
