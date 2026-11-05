import { Component, computed, inject, input, signal, effect } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Api } from '../core/api.service';
import { Movie, Show } from '../core/models';
import { Poster } from '../shared/poster';
import { dayKey, fmtDate, fmtTime, inr, runtime } from '../core/format';
import { errorText } from '../core/toast.service';

@Component({
  selector: 'app-movie-detail',
  imports: [RouterLink, Poster],
  template: `
    @if (loading()) { <div class="spinner" role="status"></div> }
    @else if (error()) { <div class="empty">{{ error() }}</div> }
    @else if (movie(); as m) {
      <div class="head">
        <app-poster class="p" [title]="m.title" [genre]="m.genre" />
        <div class="stack info">
          <a routerLink="/" class="muted">← All movies</a>
          <h1>{{ m.title }}</h1>
          <div class="row">
            <span class="badge">{{ m.genre }}</span><span class="badge">{{ m.language }}</span><span class="badge">{{ rt(m.durationMinutes) }}</span>
            <span class="muted">Released {{ m.releaseDate }}</span>
          </div>
          @if (m.description) { <p class="desc">{{ m.description }}</p> }
        </div>
      </div>

      <h2 class="sh">Showtimes</h2>
      @if (!days().length) { <div class="empty">No upcoming shows scheduled for this movie.</div> }
      @else {
        <div class="row tabs" role="tablist">
          @for (d of days(); track d.key) {
            <button role="tab" class="chip" [class.on]="d.key === activeDay()" (click)="day.set(d.key)">{{ d.label }}</button>
          }
        </div>
        <div class="stack">
          @for (t of theatresForDay(); track t.name) {
            <div class="card">
              <h3>{{ t.name }}</h3>
              <div class="row times">
                @for (s of t.shows; track s.showId) {
                  <a class="time" [routerLink]="['/shows', s.showId, 'seats']">
                    <b>{{ time(s.startTime) }}</b><small>{{ s.screenName }} · {{ money(s.ticketPrice) }}</small>
                  </a>
                }
              </div>
            </div>
          }
        </div>
      }
    }
  `,
  styles: [`
    .head { display: grid; grid-template-columns: 220px 1fr; gap: 32px; margin-bottom: 40px; align-items: start; }
    .desc { max-width: 62ch; color: var(--muted); font-size: 16px; }
    .sh { margin-bottom: 16px; } .tabs { margin-bottom: 16px; }
    .times { margin-top: 14px; }
    .time { display: grid; gap: 2px; padding: 10px 16px; border: 1px solid var(--line); border-radius: 12px; background: var(--surface-2); transition: border-color .12s, transform .12s; }
    .time:hover { border-color: var(--accent); transform: translateY(-1px); }
    .time b { color: var(--ok); } .time small { color: var(--muted); font-size: 12px; }
    @media (max-width: 640px) { .head { grid-template-columns: 120px 1fr; gap: 16px; } }
  `],
})
export class MovieDetailPage {
  private api = inject(Api);
  id = input.required<string>();           // bound from the route (withComponentInputBinding)
  movie = signal<Movie | null>(null);
  shows = signal<Show[]>([]);
  loading = signal(true); error = signal('');
  day = signal('');
  rt = runtime; time = fmtTime; money = inr;

  /** Only upcoming shows can be booked. */
  private upcoming = computed(() => this.shows().filter(s => new Date(s.startTime).getTime() > Date.now())
    .sort((a, b) => a.startTime.localeCompare(b.startTime)));
  days = computed(() => {
    const seen = new Map<string, string>();
    for (const s of this.upcoming()) if (!seen.has(dayKey(s.startTime))) seen.set(dayKey(s.startTime), fmtDate(s.startTime));
    return [...seen].map(([key, label]) => ({ key, label }));
  });
  activeDay = computed(() => this.day() || this.days()[0]?.key || '');
  theatresForDay = computed(() => {
    const m = new Map<string, Show[]>();
    for (const s of this.upcoming().filter(s => dayKey(s.startTime) === this.activeDay())) {
      m.set(s.theatreName, [...(m.get(s.theatreName) ?? []), s]);
    }
    return [...m].map(([name, shows]) => ({ name, shows }));
  });

  constructor() {
    effect(() => {
      const id = +this.id();
      this.loading.set(true); this.error.set('');
      this.api.movie(id).subscribe({
        next: m => { this.movie.set(m); this.api.showsByMovie(id).subscribe({ next: s => { this.shows.set(s); this.loading.set(false); }, error: e => { this.error.set(errorText(e)); this.loading.set(false); } }); },
        error: e => { this.error.set(e.status === 404 ? 'Movie not found.' : errorText(e)); this.loading.set(false); },
      });
    });
  }
}
