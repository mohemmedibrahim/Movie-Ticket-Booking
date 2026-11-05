import { Component, computed, effect, inject, input, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Api } from '../core/api.service';
import { Auth } from '../core/auth.service';
import { Show, ShowSeat } from '../core/models';
import { fmtDateTime, inr } from '../core/format';
import { Toasts, errorText } from '../core/toast.service';

const MAX_SEATS = 10;

@Component({
  selector: 'app-seat-select',
  imports: [FormsModule, RouterLink],
  template: `
    @if (loading()) { <div class="spinner" role="status"></div> }
    @else if (error()) { <div class="empty">{{ error() }}</div> }
    @else if (show(); as s) {
      <a [routerLink]="['/movies', s.movieId]" class="muted">← {{ s.movieTitle }}</a>
      <h1 class="t">{{ s.movieTitle }}</h1>
      <p class="muted">{{ s.theatreName }} · {{ s.screenName }} · {{ when(s.startTime) }}</p>
      @if (past()) { <p class="error">This show has already started, so it can't be booked.</p> }

      <div class="layout">
        <section class="card seats">
          <div class="screen" aria-hidden="true"><span>SCREEN</span></div>
          @for (r of rows(); track r.name) {
            <div class="seatrow">
              <span class="rn">{{ r.name }}</span>
              <div class="row-seats">
                @for (seat of r.seats; track seat.seatId) {
                  <button type="button" class="seat" [class.prem]="seat.seatType === 'Premium'"
                    [class.sel]="isSel(seat.seatId)" [disabled]="seat.status !== 'Available' || past()"
                    [attr.aria-pressed]="isSel(seat.seatId)" [attr.aria-label]="seat.seatLabel + ' ' + seat.seatType + ' ' + seat.status + ', ' + money(seat.price)"
                    [title]="seat.seatLabel + ' · ' + seat.seatType + ' · ' + money(seat.price)"
                    (click)="toggle(seat)">{{ seat.seatNumber }}</button>
                }
              </div>
            </div>
          }
          <div class="legend">
            <span><i class="seat sm"></i>Available</span>
            <span><i class="seat sm prem"></i>Premium</span>
            <span><i class="seat sm sel"></i>Selected</span>
            <span><i class="seat sm" disabled></i>Booked</span>
          </div>
        </section>

        <aside class="card side stack">
          <h3>Your booking</h3>
          @if (!selected().length) { <p class="muted">Select up to {{ max }} seats.</p> }
          @else {
            <div class="chosen">
              @for (x of selected(); track x.seatId) { <span class="badge">{{ x.seatLabel }}</span> }
            </div>
            <div class="row spread total"><span>{{ selected().length }} × ticket</span><b>{{ money(total()) }}</b></div>
          }
          <form #f="ngForm" class="stack" (ngSubmit)="book()">
            <label class="f">Name<input name="n" required maxlength="100" [(ngModel)]="name"></label>
            <label class="f">Phone<input name="p" required pattern="^\\+?[0-9]{7,14}$" [(ngModel)]="phone" placeholder="9876543210">
              <span class="hint">7–14 digits, optional leading +</span></label>
            <label class="f">Email<input name="e" type="email" maxlength="150" [(ngModel)]="email"></label>
            <button class="btn primary" [disabled]="f.invalid || !selected().length || busy() || past()">
              {{ busy() ? 'Booking…' : 'Confirm booking' + (total() ? ' · ' + money(total()) : '') }}
            </button>
            <p class="hint">Payment isn't part of this app yet — booking confirms the seats directly.</p>
          </form>
        </aside>
      </div>
    }
  `,
  styles: [`
    .t { margin: 8px 0 6px; font-size: clamp(26px, 4vw, 38px); }
    .layout { display: grid; grid-template-columns: 1fr 340px; gap: 24px; margin-top: 24px; align-items: start; }
    @media (max-width: 900px) { .layout { grid-template-columns: 1fr; } }
    .seats { overflow-x: auto; }
    .screen { height: 36px; margin: 0 auto 28px; width: min(480px, 80%); border-top: 4px solid var(--accent); border-radius: 50% 50% 0 0 / 100% 100% 0 0; background: linear-gradient(var(--accent) -400%, transparent); text-align: center; color: var(--muted); font-size: 11px; letter-spacing: .3em; padding-top: 8px; }
    .seatrow { display: flex; align-items: center; gap: 10px; justify-content: center; margin-bottom: 8px; min-width: max-content; }
    .rn { width: 22px; color: var(--muted); font-weight: 600; font-size: 13px; text-align: center; }
    .row-seats { display: flex; gap: 6px; }
    .seat { width: 34px; height: 32px; border-radius: 8px 8px 4px 4px; border: 1px solid var(--line); background: var(--surface-2); color: var(--muted); font: 600 11px var(--font-body); cursor: pointer; padding: 0; transition: transform .1s; display: inline-block; }
    .seat:hover:not(:disabled) { transform: translateY(-2px); border-color: var(--accent); }
    .seat.prem { border-color: var(--premium); color: var(--premium); }
    .seat.sel { background: var(--accent); border-color: var(--accent); color: var(--accent-ink); }
    .seat:disabled, .seat[disabled] { background: repeating-linear-gradient(45deg, transparent 0 4px, var(--line) 4px 5px); opacity: .5; cursor: not-allowed; }
    .legend { display: flex; gap: 18px; justify-content: center; flex-wrap: wrap; margin-top: 24px; font-size: 12px; color: var(--muted); }
    .legend span { display: flex; align-items: center; gap: 6px; } .seat.sm { width: 18px; height: 16px; cursor: default; }
    .side { position: sticky; top: 84px; }
    .chosen { display: flex; gap: 6px; flex-wrap: wrap; }
    .total { font-size: 17px; border-top: 1px dashed var(--line); padding-top: 12px; }
  `],
})
export class SeatSelectPage {
  private api = inject(Api); private auth = inject(Auth); private router = inject(Router); private toast = inject(Toasts);
  id = input.required<string>();
  show = signal<Show | null>(null);
  seats = signal<ShowSeat[]>([]);
  picked = signal<number[]>([]);
  loading = signal(true); error = signal(''); busy = signal(false);
  name = this.auth.user()?.fullName ?? ''; phone = this.auth.user()?.phone ?? ''; email = this.auth.user()?.email ?? '';
  max = MAX_SEATS; money = inr; when = fmtDateTime;

  past = computed(() => !!this.show() && new Date(this.show()!.startTime).getTime() <= Date.now());
  rows = computed(() => {
    const m = new Map<string, ShowSeat[]>();
    for (const s of this.seats()) m.set(s.rowName, [...(m.get(s.rowName) ?? []), s]);
    return [...m].sort(([a], [b]) => a.localeCompare(b, undefined, { numeric: true }))
      .map(([name, seats]) => ({ name, seats: seats.sort((a, b) => a.seatNumber - b.seatNumber) }));
  });
  selected = computed(() => this.seats().filter(s => this.picked().includes(s.seatId)));
  total = computed(() => this.selected().reduce((n, s) => n + s.price, 0));
  isSel = (id: number) => this.picked().includes(id);

  constructor() {
    effect(() => {
      const id = +this.id();
      this.loading.set(true); this.error.set('');
      this.api.show(id).subscribe({
        next: s => { this.show.set(s); this.loadSeats(() => this.loading.set(false)); },
        error: e => { this.error.set(e.status === 404 ? 'Show not found.' : errorText(e)); this.loading.set(false); },
      });
    });
  }
  private loadSeats(done?: () => void) {
    this.api.showSeats(+this.id()).subscribe({
      next: s => { this.seats.set(s); this.picked.update(p => p.filter(id => s.find(x => x.seatId === id)?.status === 'Available')); done?.(); },
      error: e => { this.error.set(errorText(e)); done?.(); },
    });
  }

  toggle(seat: ShowSeat) {
    this.picked.update(p => {
      if (p.includes(seat.seatId)) return p.filter(x => x !== seat.seatId);
      if (p.length >= MAX_SEATS) { this.toast.err(`You can book at most ${MAX_SEATS} seats at once.`); return p; }
      return [...p, seat.seatId];
    });
  }

  book() {
    if (!this.selected().length) return;
    this.busy.set(true);
    this.api.createBooking({
      showId: +this.id(), seatIds: this.picked(),
      customerName: this.name.trim(), customerPhone: this.phone.trim(), customerEmail: this.email.trim() || undefined,
    }).subscribe({
      next: b => { this.toast.ok(`Booked! Reference ${b.bookingReference}`); this.router.navigate(['/bookings', b.bookingId]); },
      error: e => {
        this.busy.set(false);
        this.toast.err(errorText(e));
        if (e.status === 409) this.loadSeats(); // someone grabbed a seat: refresh the map
      },
    });
  }
}
