import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Api } from '../../core/api.service';
import { Movie, MovieInput } from '../../core/models';
import { runtime } from '../../core/format';
import { Toasts, errorText } from '../../core/toast.service';

const blank = (): MovieInput => ({ title: '', description: '', durationMinutes: 120, releaseDate: '', language: '', genre: '' });

@Component({
  selector: 'app-admin-movies',
  imports: [FormsModule],
  template: `
    <div class="row spread" style="margin-bottom:16px"><h2>Movies</h2><button class="btn primary" (click)="open()">Add movie</button></div>
    @if (loading()) { <div class="spinner"></div> }
    @else if (!items().length) { <div class="empty">No movies yet.</div> }
    @else {
      <div class="tbl-wrap"><table>
        <thead><tr><th>Title</th><th>Genre</th><th>Language</th><th>Runtime</th><th>Release</th><th></th></tr></thead>
        <tbody>@for (m of items(); track m.movieId) {
          <tr><td><b>{{ m.title }}</b></td><td>{{ m.genre }}</td><td>{{ m.language }}</td><td>{{ rt(m.durationMinutes) }}</td><td>{{ m.releaseDate }}</td>
            <td class="actions"><button class="btn sm" (click)="open(m)">Edit</button> <button class="btn sm danger" (click)="remove(m)">Delete</button></td></tr>
        }</tbody></table></div>
    }
    @if (form(); as f) {
      <div class="scrim" (click)="form.set(null)"><form #fm="ngForm" class="modal stack" (click)="$event.stopPropagation()" (ngSubmit)="save()">
        <h2>{{ editId() ? 'Edit' : 'New' }} movie</h2>
        <label class="f">Title<input name="t" required maxlength="150" [(ngModel)]="f.title"></label>
        <label class="f">Description<textarea name="d" maxlength="500" [(ngModel)]="f.description"></textarea></label>
        <div class="grid2">
          <label class="f">Genre<input name="g" required maxlength="50" [(ngModel)]="f.genre"></label>
          <label class="f">Language<input name="l" required maxlength="50" [(ngModel)]="f.language"></label>
          <label class="f">Duration (minutes)<input name="m" type="number" min="1" required [(ngModel)]="f.durationMinutes"></label>
          <label class="f">Release date<input name="r" type="date" required [(ngModel)]="f.releaseDate"></label>
        </div>
        <div class="row" style="justify-content:flex-end"><button type="button" class="btn" (click)="form.set(null)">Cancel</button>
          <button class="btn primary" [disabled]="fm.invalid || busy()">Save</button></div>
      </form></div>
    }
  `,
})
export class AdminMovies {
  private api = inject(Api); private toast = inject(Toasts);
  items = signal<Movie[]>([]); loading = signal(true); busy = signal(false);
  form = signal<MovieInput | null>(null); editId = signal<number | null>(null); rt = runtime;
  constructor() { this.load(); }
  load() { this.api.movies(1, 100).subscribe({ next: r => { this.items.set(r.items); this.loading.set(false); }, error: e => { this.toast.err(errorText(e)); this.loading.set(false); } }); }
  open(m?: Movie) { this.editId.set(m?.movieId ?? null); this.form.set(m ? { ...m } : blank()); }
  save() {
    const f = this.form()!; this.busy.set(true);
    const body = { ...f, description: f.description || null };
    const req = this.editId() ? this.api.updateMovie(this.editId()!, body) : this.api.createMovie(body);
    req.subscribe({ next: () => { this.busy.set(false); this.form.set(null); this.toast.ok('Saved.'); this.load(); }, error: e => { this.busy.set(false); this.toast.err(errorText(e)); } });
  }
  remove(m: Movie) {
    if (!confirm(`Delete "${m.title}"?`)) return;
    this.api.deleteMovie(m.movieId).subscribe({ next: () => { this.toast.ok('Deleted.'); this.load(); }, error: e => this.toast.err(errorText(e)) });
  }
}
