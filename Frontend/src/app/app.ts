import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { Auth } from './core/auth.service';
import { Toasts } from './core/toast.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  auth = inject(Auth);
  toasts = inject(Toasts);
  router = inject(Router);
  menu = signal(false);
  theme = signal<'dark' | 'light'>(this.initTheme());

  private initTheme(): 'dark' | 'light' {
    try { const t = localStorage.getItem('mtb.theme'); if (t === 'light' || t === 'dark') { document.documentElement.dataset['theme'] = t; return t; } } catch { /* ignore */ }
    return 'dark';
  }
  toggleTheme() {
    const t = this.theme() === 'dark' ? 'light' : 'dark';
    this.theme.set(t);
    document.documentElement.dataset['theme'] = t;
    try { localStorage.setItem('mtb.theme', t); } catch { /* ignore */ }
  }
}
