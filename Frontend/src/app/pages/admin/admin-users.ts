import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Api } from '../../core/api.service';
import { Toasts, errorText } from '../../core/toast.service';

@Component({
  selector: 'app-admin-users',
  imports: [FormsModule],
  template: `
    <h2 style="margin-bottom:16px">Add another admin</h2>
    <form #f="ngForm" class="card stack" style="max-width:460px" (ngSubmit)="go(f)">
      <label class="f">Full name<input name="n" required maxlength="100" [(ngModel)]="v.fullName"></label>
      <label class="f">Email<input name="e" type="email" required [(ngModel)]="v.email"></label>
      <label class="f">Phone<input name="p" required pattern="^\\+?[0-9]{7,14}$" [(ngModel)]="v.phone"></label>
      <label class="f">Password<input name="pw" type="password" required minlength="6" [(ngModel)]="v.password"><span class="hint">Upper & lower case, a digit and a symbol.</span></label>
      <button class="btn primary" [disabled]="f.invalid || busy()">Create admin</button>
    </form>
  `,
})
export class AdminUsers {
  private api = inject(Api); private toast = inject(Toasts);
  v = { fullName: '', email: '', phone: '', password: '' }; busy = signal(false);
  go(f: any) {
    this.busy.set(true);
    this.api.registerAdmin(this.v).subscribe({
      next: u => { this.busy.set(false); this.toast.ok(`Admin ${u.email} created.`); this.v = { fullName: '', email: '', phone: '', password: '' }; f.resetForm(this.v); },
      error: e => { this.busy.set(false); this.toast.err(errorText(e)); },
    });
  }
}
