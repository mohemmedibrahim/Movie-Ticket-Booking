import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Api } from '../../core/api.service';
import { Movie, Screen, Show } from '../../core/models';
import { fmtDateTime, fromLocalInput, inr, toLocalInput } from '../../core/format';
import { Toasts, errorText } from '../../core/toast.service';

interface Draft { movieId: number; screenId: number; start: string; end: string; price: number; }

@Component({
  selector: 'app-admin-shows',
  imports: [FormsModule],
  template: `
    <div class="row spread" style="margin-bottom:16px"><h2>Shows</h2><button class="btn primary" (click)="open()">Schedule show</button></div>
    <div class="row" style="margin-bottom:16px">
      <label class="f">Movie<select [ngModel]="fMovie()" (ngModelChange)="fMovie.set($event); page.set(1); load()"><option [ngValue]="0">All</option>
        @for (m of movies(); track m.movieId) { <option [ngValue]="m.movieId">{{ m.title }}</option> }</select></label>
      <label class="f">Date<input type="date" [ngModel]="fDate()" (ngModelChange)="fDate.set($event); page.set(1); load()"></label>
    </div>
    @if (loading()) { <div class="spinner"></div> }
    @else if (!items().length) { <div class="empty">No shows found.</div> }
    @else {
      <div class="tbl-wrap"><table>
        <thead><tr><th>Movie</th><th>Theatre / screen</th><th>Starts</th><th>Ends</th><th>Price</th><th></th></tr></thead>
        <tbody>@for (s of items(); track s.showId) {
          <tr><td><b>{{ s.movieTitle }}</b></td><td>{{ s.theatreName }} · {{ s.screenName }}</td><td>{{ when(s.startTime) }}</td><td>{{ when(s.endTime) }}</td><td>{{ money(s.ticketPrice) }}</td>
            <td class="actions"><button class="btn sm" (click)="open(s)">Edit</button> <button class="btn sm danger" (click)="remove(s)">Delete</button></td></tr>
        }</tbody></table></div>
      <div class="row" style="margin-top:14px;justify-content:center">
        <button class="btn sm" [disabled]="page() <= 1" (click)="go(-1)">← Prev</button>
        <span class="muted">Page {{ page() }} of {{ pages() || 1 }}</span>
        <button class="btn sm" [disabled]="page() >= pages()" (click)="go(1)">Next →</button>
      </div>
    }
    @if (form(); as f) {
      <div class="scrim" (click)="form.set(null)"><form #fm="ngForm" class="modal stack" (click)="$event.stopPropagation()" (ngSubmit)="save()">
        <h2>{{ editId() ? 'Edit' : 'Schedule' }} show</h2>
        <label class="f">Movie<select name="m" required [(ngModel)]="f.movieId" (ngModelChange)="autoEnd()"><option [ngValue]="0" disabled>Select…</option>
          @for (m of movies(); track m.movieId) { <option [ngValue]="m.movieId">{{ m.title }}</option> }</select></label>
        <label class="f">Screen
          <select name="s" required [(ngModel)]="f.screenId" [disabled]="!!editId()"><option [ngValue]="0" disabled>Select…</option>
            @for (s of screens(); track s.screenId) { <option [ngValue]="s.screenId">{{ s.theatreName }} — {{ s.screenName }}</option> }</select>
          @if (editId()) { <span class="hint">A show's screen can't be changed after creation.</span> }</label>
        <div class="grid2">
          <label class="f">Start<input name="st" type="datetime-local" required [(ngModel)]="f.start" (ngModelChange)="autoEnd()"></label>
          <label class="f">End<input name="en" type="datetime-local" required [(ngModel)]="f.end"></label>
        </div>
        <label class="f">Ticket price (₹)<input name="p" type="number" min="0" max="100000" required [(ngModel)]="f.price"></label>
        @if (f.start && f.end && f.end <= f.start) { <p class="error">End must be after start.</p> }
        <div class="row" style="justify-content:flex-end"><button type="button" class="btn" (click)="form.set(null)">Cancel</button>
          <button class="btn primary" [disabled]="fm.invalid || !f.movieId || !f.screenId || f.end <= f.start || busy()">Save</button></div>
      </form></div>
    }
  `,
})
export class AdminShows {
  private api = inject(Api); private toast = inject(Toasts);
  items = signal<Show[]>([]); movies = signal<Movie[]>([]); screens = signal<Screen[]>([]);
  loading = signal(true); busy = signal(false); page = signal(1); pages = signal(1);
  fMovie = signal(0); fDate = signal('');
  form = signal<Draft | null>(null); editId = signal<number | null>(null);
  money = inr; when = fmtDateTime;

  constructor() {
    this.api.movies(1, 100).subscribe(r => this.movies.set(r.items));
    this.api.screens().subscribe(s => this.screens.set(s));
    this.load();
  }
  load() {
    this.loading.set(true);
    this.api.shows({ movieId: this.fMovie() || undefined, date: this.fDate() || undefined, pageNumber: this.page(), pageSize: 15 }).subscribe({
      next: r => { this.items.set(r.items); this.pages.set(r.totalPages); this.loading.set(false); },
      error: e => { this.toast.err(errorText(e)); this.loading.set(false); },
    });
  }
  go(d: number) { this.page.update(p => p + d); this.load(); }
  open(s?: Show) {
    this.editId.set(s?.showId ?? null);
    this.form.set(s ? { movieId: s.movieId, screenId: s.screenId, start: toLocalInput(s.startTime), end: toLocalInput(s.endTime), price: s.ticketPrice }
                    : { movieId: 0, screenId: 0, start: '', end: '', price: 200 });
  }
  /** Pre-fill the end time from the movie's runtime (+15 min buffer) until the admin edits it. */
  autoEnd() {
    const f = this.form(); if (!f || !f.start || !f.movieId || this.editId()) return;
    const m = this.movies().find(x => x.movieId === f.movieId); if (!m) return;
    const d = new Date(f.start); d.setMinutes(d.getMinutes() + m.durationMinutes + 15);
    f.end = this.localInput(d);
  }
  private localInput(d: Date) {
    const p = (n: number) => String(n).padStart(2, '0');
    return `${d.getFullYear()}-${p(d.getMonth() + 1)}-${p(d.getDate())}T${p(d.getHours())}:${p(d.getMinutes())}`;
  }
  save() {
    const f = this.form()!; this.busy.set(true);
    const body = { movieId: f.movieId, startTime: fromLocalInput(f.start), endTime: fromLocalInput(f.end), ticketPrice: +f.price };
    const req = this.editId() ? this.api.updateShow(this.editId()!, body) : this.api.createShow({ ...body, screenId: f.screenId });
    req.subscribe({ next: () => { this.busy.set(false); this.form.set(null); this.toast.ok('Saved.'); this.load(); }, error: e => { this.busy.set(false); this.toast.err(errorText(e)); } });
  }
  remove(s: Show) {
    if (!confirm(`Delete this show of "${s.movieTitle}"?`)) return;
    this.api.deleteShow(s.showId).subscribe({ next: () => { this.toast.ok('Deleted.'); this.load(); }, error: e => this.toast.err(errorText(e)) });
  }
}
