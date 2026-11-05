import { Component, inject, input, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Auth } from '../core/auth.service';
import { errorText } from '../core/toast.service';

const PW = '^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^A-Za-z0-9]).{6,100}$';

@Component({
  selector: 'app-login',
  imports: [FormsModule, RouterLink],
  template: `
    <form #f="ngForm" class="card stack box" (ngSubmit)="go()">
      <h2>Welcome back</h2>
      <label class="f">Email<input name="e" type="email" required autocomplete="email" [(ngModel)]="email"></label>
      <label class="f">Password<input name="p" type="password" required autocomplete="current-password" [(ngModel)]="password"></label>
      @if (error()) { <p class="error" role="alert">{{ error() }}</p> }
      <button class="btn primary" [disabled]="f.invalid || busy()">{{ busy() ? 'Signing in…' : 'Sign in' }}</button>
      <p class="muted">New here? <a routerLink="/register" [queryParams]="{ returnUrl: returnUrl() }" class="link">Create an account</a></p>
    </form>
  `,
  styles: [`.box { max-width: 420px; margin: 40px auto; } .link { color: var(--accent); }`],
})
export class LoginPage {
  private auth = inject(Auth); private router = inject(Router);
  returnUrl = input<string>('/');
  email = ''; password = ''; busy = signal(false); error = signal('');
  go() {
    this.busy.set(true); this.error.set('');
    this.auth.login(this.email.trim(), this.password).subscribe({
      next: () => this.router.navigateByUrl(this.returnUrl() || '/'),
      error: e => { this.busy.set(false); this.error.set(e.status === 401 ? 'Invalid email or password (5 wrong tries locks the account for 10 minutes).' : errorText(e)); },
    });
  }
}

@Component({
  selector: 'app-register',
  imports: [FormsModule, RouterLink],
  template: `
    <form #f="ngForm" class="card stack box" (ngSubmit)="go()">
      <h2>Create your account</h2>
      <label class="f">Full name<input name="n" required maxlength="100" autocomplete="name" [(ngModel)]="v.fullName"></label>
      <label class="f">Email<input name="e" type="email" required maxlength="150" autocomplete="email" [(ngModel)]="v.email"></label>
      <label class="f">Phone<input name="ph" required pattern="^\\+?[0-9]{7,14}$" autocomplete="tel" [(ngModel)]="v.phone"><span class="hint">7–14 digits, optional leading +</span></label>
      <label class="f">Password<input name="p" type="password" required [pattern]="pw" autocomplete="new-password" [(ngModel)]="v.password">
        <span class="hint">6+ characters with upper & lower case, a digit and a symbol.</span></label>
      @if (error()) { <p class="error" role="alert">{{ error() }}</p> }
      <button class="btn primary" [disabled]="f.invalid || busy()">{{ busy() ? 'Creating…' : 'Create account' }}</button>
      <p class="muted">Already registered? <a routerLink="/login" [queryParams]="{ returnUrl: returnUrl() }" class="link">Sign in</a></p>
    </form>
  `,
  styles: [`.box { max-width: 420px; margin: 40px auto; } .link { color: var(--accent); }`],
})
export class RegisterPage {
  private auth = inject(Auth); private router = inject(Router);
  returnUrl = input<string>('/');
  pw = PW;
  v = { fullName: '', email: '', phone: '', password: '' };
  busy = signal(false); error = signal('');
  go() {
    this.busy.set(true); this.error.set('');
    this.auth.register({ ...this.v, email: this.v.email.trim(), fullName: this.v.fullName.trim() }).subscribe({
      next: () => this.router.navigateByUrl(this.returnUrl() || '/'),
      error: e => { this.busy.set(false); this.error.set(errorText(e)); },
    });
  }
}
