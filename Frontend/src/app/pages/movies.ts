import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Api } from '../core/api.service';
import { Movie } from '../core/models';
import { Poster } from '../shared/poster';
import { runtime } from '../core/format';
import { errorText } from '../core/toast.service';

@Component({
  selector: 'app-movies',
  imports: [RouterLink, FormsModule, Poster],
  template: `
    <section class="hero">
      <p class="eyebrow">Now showing</p>
      <h1>Pick a film.<br>Pick your seat.</h1>
    </section>

    <div class="filters">
      <input type="search" placeholder="Search by title…" [ngModel]="q()" (ngModelChange)="q.set($event)" aria-label="Search movies">
      <div class="row">
        <button class="chip" [class.on]="!genre()" (click)="genre.set('')">All</button>
        @for (g of genres(); track g) { <button class="chip" [class.on]="genre() === g" (click)="genre.set(g)">{{ g }}</button> }
      </div>
      @if (languages().length > 1) {
        <div class="row">
          <button class="chip" [class.on]="!lang()" (click)="lang.set('')">Any language</button>
          @for (l of languages(); track l) { <button class="chip" [class.on]="lang() === l" (click)="lang.set(l)">{{ l }}</button> }
        </div>
      }
    </div>

    @if (loading()) { <div class="spinner" role="status" aria-label="Loading"></div> }
    @else if (error()) { <div class="empty">{{ error() }} <br><br><button class="btn" (click)="load()">Try again</button></div> }
    @else if (!filtered().length) { <div class="empty">No movies match. @if (all().length === 0) { An admin needs to add some movies first. }</div> }
    @else {
      <div class="grid">
        @for (m of filtered(); track m.movieId) {
          <a class="mv" [routerLink]="['/movies', m.movieId]">
            <app-poster [title]="m.title" [genre]="m.genre" />
            <h3>{{ m.title }}</h3>
            <p class="muted">{{ m.language }} · {{ rt(m.durationMinutes) }}</p>
          </a>
        }
      </div>
    }
  `,
  styles: [`
    .hero { margin: 8px 0 28px; }
    .eyebrow { color: var(--accent); font-weight: 600; letter-spacing: .14em; text-transform: uppercase; font-size: 12px; margin-bottom: 10px; }
    .filters { display: grid; gap: 12px; margin-bottom: 28px; }
    .filters input { max-width: 420px; }
    .grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(170px, 1fr)); gap: 22px; }
    .mv { display: grid; gap: 4px; align-content: start; }
    .mv app-poster { margin-bottom: 8px; transition: transform .18s; }
    .mv:hover app-poster { transform: translateY(-4px); }
    .mv h3 { font-size: 16px; } .mv p { font-size: 13px; }
  `],
})
export class MoviesPage {
  private api = inject(Api);
  all = signal<Movie[]>([]);
  loading = signal(true);
  error = signal('');
  q = signal(''); genre = signal(''); lang = signal('');
  rt = runtime;

  genres = computed(() => [...new Set(this.all().map(m => m.genre))].sort());
  languages = computed(() => [...new Set(this.all().map(m => m.language))].sort());
  filtered = computed(() => {
    const q = this.q().trim().toLowerCase();
    return this.all().filter(m =>
      (!q || m.title.toLowerCase().includes(q)) && (!this.genre() || m.genre === this.genre()) && (!this.lang() || m.language === this.lang()));
  });

  constructor() { this.load(); }
  load() {
    this.loading.set(true); this.error.set('');
    this.api.movies(1, 100).subscribe({
      next: r => { this.all.set(r.items); this.loading.set(false); },
      error: e => { this.error.set(errorText(e)); this.loading.set(false); },
    });
  }
}
