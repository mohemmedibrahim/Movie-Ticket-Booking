import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Api } from '../../core/api.service';
import { Theatre, TheatreInput } from '../../core/models';
import { Toasts, errorText } from '../../core/toast.service';

@Component({
  selector: 'app-admin-theatres',
  imports: [FormsModule],
  template: `
    <div class="row spread" style="margin-bottom:16px"><h2>Theatres</h2><button class="btn primary" (click)="open()">Add theatre</button></div>
    @if (loading()) { <div class="spinner"></div> }
    @else if (!items().length) { <div class="empty">No theatres yet.</div> }
    @else {
      <div class="tbl-wrap"><table>
        <thead><tr><th>Name</th><th>Location</th><th></th></tr></thead>
        <tbody>@for (t of items(); track t.theatreId) {
          <tr><td><b>{{ t.name }}</b></td><td>{{ t.location }}</td>
            <td class="actions"><button class="btn sm" (click)="open(t)">Edit</button> <button class="btn sm danger" (click)="remove(t)">Delete</button></td></tr>
        }</tbody></table></div>
    }
    @if (form(); as f) {
      <div class="scrim" (click)="form.set(null)"><form #fm="ngForm" class="modal stack" (click)="$event.stopPropagation()" (ngSubmit)="save()">
        <h2>{{ editId() ? 'Edit' : 'New' }} theatre</h2>
        <label class="f">Name<input name="n" required maxlength="150" [(ngModel)]="f.name"></label>
        <label class="f">Location<input name="l" required maxlength="200" [(ngModel)]="f.location"></label>
        <div class="row" style="justify-content:flex-end"><button type="button" class="btn" (click)="form.set(null)">Cancel</button>
          <button class="btn primary" [disabled]="fm.invalid || busy()">Save</button></div>
      </form></div>
    }
  `,
})
export class AdminTheatres {
  private api = inject(Api); private toast = inject(Toasts);
  items = signal<Theatre[]>([]); loading = signal(true); busy = signal(false);
  form = signal<TheatreInput | null>(null); editId = signal<number | null>(null);
  constructor() { this.load(); }
  load() { this.api.theatres().subscribe({ next: r => { this.items.set(r); this.loading.set(false); }, error: e => { this.toast.err(errorText(e)); this.loading.set(false); } }); }
  open(t?: Theatre) { this.editId.set(t?.theatreId ?? null); this.form.set(t ? { name: t.name, location: t.location } : { name: '', location: '' }); }
  save() {
    this.busy.set(true);
    const req = this.editId() ? this.api.updateTheatre(this.editId()!, this.form()!) : this.api.createTheatre(this.form()!);
    req.subscribe({ next: () => { this.busy.set(false); this.form.set(null); this.toast.ok('Saved.'); this.load(); }, error: e => { this.busy.set(false); this.toast.err(errorText(e)); } });
  }
  remove(t: Theatre) {
    if (!confirm(`Delete "${t.name}"?`)) return;
    this.api.deleteTheatre(t.theatreId).subscribe({ next: () => { this.toast.ok('Deleted.'); this.load(); }, error: e => this.toast.err(errorText(e)) });
  }
}
