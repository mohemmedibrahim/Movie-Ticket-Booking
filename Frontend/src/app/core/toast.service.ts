import { Injectable, signal } from '@angular/core';

export interface Toast { id: number; kind: 'ok' | 'err'; text: string; }

@Injectable({ providedIn: 'root' })
export class Toasts {
  readonly list = signal<Toast[]>([]);
  private n = 0;
  ok(text: string) { this.push('ok', text); }
  err(text: string) { this.push('err', text, 6000); }
  private push(kind: Toast['kind'], text: string, ms = 3500) {
    const id = ++this.n;
    this.list.update(l => [...l, { id, kind, text }]);
    setTimeout(() => this.list.update(l => l.filter(t => t.id !== id)), ms);
  }
}

/** Turns an HttpErrorResponse (ProblemDetails / validation errors) into one readable line. */
export function errorText(e: any): string {
  if (e?.status === 0) return 'Cannot reach the server. Is the API running on https://localhost:7100 and is its certificate trusted?';
  const b = e?.error;
  if (b?.errors && typeof b.errors === 'object') {
    const msgs = Object.values(b.errors).flat() as string[];
    if (msgs.length) return msgs.join(' ');
  }
  if (b?.detail) return b.detail;
  if (e?.status === 401) return 'Please sign in to continue.';
  if (e?.status === 403) return 'You do not have permission to do that.';
  return b?.title || e?.message || 'Something went wrong.';
}
