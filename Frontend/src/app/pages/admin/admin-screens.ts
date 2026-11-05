import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Api } from '../../core/api.service';
import { BulkSeatRow, Screen, Seat, Theatre } from '../../core/models';
import { Toasts, errorText } from '../../core/toast.service';

@Component({
  selector: 'app-admin-screens',
  imports: [FormsModule],
  template: `
    <div class="row spread" style="margin-bottom:16px"><h2>Screens & seats</h2></div>
    <div class="row" style="margin-bottom:16px">
      <label class="f grow" style="max-width:280px">Theatre
        <select [ngModel]="theatreId()" (ngModelChange)="pick($event)">
          <option [ngValue]="0" disabled>Select a theatre…</option>
          @for (t of theatres(); track t.theatreId) { <option [ngValue]="t.theatreId">{{ t.name }} — {{ t.location }}</option> }
        </select></label>
    </div>

    @if (theatreId()) {
      <form class="row card" style="margin-bottom:16px" (ngSubmit)="add()" #sf="ngForm">
        <label class="f grow">New screen name<input name="s" required maxlength="50" [(ngModel)]="newName" placeholder="Screen 1"></label>
        <button class="btn primary" style="align-self:flex-end" [disabled]="sf.invalid">Add screen</button>
      </form>
      @if (!screens().length) { <div class="empty">This theatre has no screens yet.</div> }
      @else {
        <div class="tbl-wrap"><table>
          <thead><tr><th>Screen</th><th></th></tr></thead>
          <tbody>@for (s of screens(); track s.screenId) {
            <tr><td><b>{{ s.screenName }}</b></td>
              <td class="actions"><button class="btn sm" (click)="openSeats(s)">Seats</button> <button class="btn sm" (click)="rename(s)">Rename</button> <button class="btn sm danger" (click)="remove(s)">Delete</button></td></tr>
          }</tbody></table></div>
      }
    }

    @if (seatScreen(); as sc) {
      <div class="card stack" style="margin-top:24px">
        <div class="row spread"><h3>{{ sc.screenName }} — seats ({{ seats().length }})</h3><button class="btn sm" (click)="seatScreen.set(null)">Close</button></div>
        <form class="stack" (ngSubmit)="bulk()" #bf="ngForm">
          <p class="muted">Add rows in bulk. Seats can't be added once a screen has shows scheduled.</p>
          @for (r of rowsDraft; track $index) {
            <div class="row">
              <label class="f" style="width:90px">Row<input [name]="'r'+$index" required maxlength="5" [(ngModel)]="r.rowName" placeholder="A"></label>
              <label class="f" style="width:110px">Seats<input [name]="'c'+$index" type="number" min="1" max="100" required [(ngModel)]="r.seatCount"></label>
              <label class="f" style="width:140px">Type<select [name]="'t'+$index" [(ngModel)]="r.seatType"><option>Regular</option><option>Premium</option></select></label>
              <button type="button" class="btn sm ghost" style="align-self:flex-end" (click)="rowsDraft.splice($index, 1)" [disabled]="rowsDraft.length === 1">Remove</button>
            </div>
          }
          <div class="row"><button type="button" class="btn sm" (click)="addRow()">+ Row</button><button class="btn primary sm" [disabled]="bf.invalid">Create seats</button></div>
        </form>
        @if (seats().length) {
          <div class="stack" style="gap:8px">
            @for (r of seatRows(); track r[0]) {
              <div class="row" style="gap:6px"><b style="width:24px">{{ r[0] }}</b>
                @for (x of r[1]; track x.seatId) { <button class="btn sm ghost" [class.prem]="x.seatType === 'Premium'" [title]="'Click to delete ' + x.seatLabel" (click)="delSeat(x)">{{ x.seatNumber }}</button> }
              </div>
            }
            <p class="hint">Click a seat to delete it. Purple outline = Premium.</p>
          </div>
        }
      </div>
    }
  `,
  styles: [`.prem { border-color: var(--premium); color: var(--premium); }`],
})
export class AdminScreens {
  private api = inject(Api); private toast = inject(Toasts);
  theatres = signal<Theatre[]>([]); screens = signal<Screen[]>([]); theatreId = signal(0);
  seatScreen = signal<Screen | null>(null); seats = signal<Seat[]>([]);
  newName = '';
  rowsDraft: BulkSeatRow[] = [{ rowName: 'A', seatCount: 10, seatType: 'Regular' }];

  seatRows = computed(() => {
    const m = new Map<string, Seat[]>();
    for (const s of this.seats()) m.set(s.rowName, [...(m.get(s.rowName) ?? []), s]);
    return [...m].sort(([a], [b]) => a.localeCompare(b, undefined, { numeric: true })).map(([k, v]) => [k, v.sort((a, b) => a.seatNumber - b.seatNumber)] as const);
  });

  constructor() { this.api.theatres().subscribe({ next: t => this.theatres.set(t), error: e => this.toast.err(errorText(e)) }); }
  pick(id: number) { this.theatreId.set(id); this.seatScreen.set(null); this.loadScreens(); }
  loadScreens() { this.api.screens(this.theatreId()).subscribe({ next: s => this.screens.set(s), error: e => this.toast.err(errorText(e)) }); }
  add() { this.api.createScreen({ theatreId: this.theatreId(), screenName: this.newName.trim() }).subscribe({ next: () => { this.newName = ''; this.toast.ok('Screen added.'); this.loadScreens(); }, error: e => this.toast.err(errorText(e)) }); }
  rename(s: Screen) {
    const name = prompt('New screen name', s.screenName)?.trim();
    if (!name || name === s.screenName) return;
    this.api.updateScreen(s.screenId, { screenName: name }).subscribe({ next: () => { this.toast.ok('Renamed.'); this.loadScreens(); }, error: e => this.toast.err(errorText(e)) });
  }
  remove(s: Screen) {
    if (!confirm(`Delete ${s.screenName}?`)) return;
    this.api.deleteScreen(s.screenId).subscribe({ next: () => { this.toast.ok('Deleted.'); this.seatScreen.set(null); this.loadScreens(); }, error: e => this.toast.err(errorText(e)) });
  }
  openSeats(s: Screen) { this.seatScreen.set(s); this.loadSeats(); }
  loadSeats() { this.api.seatsByScreen(this.seatScreen()!.screenId).subscribe({ next: s => this.seats.set(s), error: e => this.toast.err(errorText(e)) }); }
  addRow() { this.rowsDraft.push({ rowName: String.fromCharCode(65 + this.rowsDraft.length), seatCount: 10, seatType: 'Regular' }); }
  bulk() {
    this.api.bulkSeats({ screenId: this.seatScreen()!.screenId, rows: this.rowsDraft.map(r => ({ ...r, rowName: r.rowName.trim().toUpperCase(), seatCount: +r.seatCount })) })
      .subscribe({ next: () => { this.toast.ok('Seats created.'); this.rowsDraft = [{ rowName: '', seatCount: 10, seatType: 'Regular' }]; this.loadSeats(); }, error: e => this.toast.err(errorText(e)) });
  }
  delSeat(x: Seat) {
    if (!confirm(`Delete seat ${x.seatLabel}?`)) return;
    this.api.deleteSeat(x.seatId).subscribe({ next: () => this.loadSeats(), error: e => this.toast.err(errorText(e)) });
  }
}
