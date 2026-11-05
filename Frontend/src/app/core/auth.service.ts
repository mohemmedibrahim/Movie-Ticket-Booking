import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { tap } from 'rxjs';
import { Api } from './api.service';
import { AuthResponse, User } from './models';

const KEY = 'mtb.auth';

@Injectable({ providedIn: 'root' })
export class Auth {
  private api = inject(Api);
  private router = inject(Router);
  private state = signal<{ token: string; expiresAtUtc: string; user: User } | null>(this.load());

  readonly user = computed(() => this.state()?.user ?? null);
  readonly token = computed(() => this.state()?.token ?? null);
  readonly isLoggedIn = computed(() => !!this.state());
  readonly isAdmin = computed(() => !!this.state()?.user.roles.includes('Admin'));

  private load() {
    try {
      const raw = localStorage.getItem(KEY);
      if (!raw) return null;
      const s = JSON.parse(raw);
      return new Date(s.expiresAtUtc).getTime() > Date.now() ? s : null; // drop expired tokens
    } catch { return null; }
  }
  private persist(r: AuthResponse) {
    const s = { token: r.token, expiresAtUtc: r.expiresAtUtc, user: r.user };
    this.state.set(s);
    try { localStorage.setItem(KEY, JSON.stringify(s)); } catch { /* storage blocked */ }
  }

  login(email: string, password: string) { return this.api.login(email, password).pipe(tap(r => this.persist(r))); }
  register(b: { fullName: string; email: string; phone: string; password: string }) { return this.api.register(b).pipe(tap(r => this.persist(r))); }

  logout(redirect = true) {
    this.state.set(null);
    try { localStorage.removeItem(KEY); } catch { /* ignore */ }
    if (redirect) this.router.navigate(['/']);
  }
}
