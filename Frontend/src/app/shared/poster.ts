import { Component, computed, input } from '@angular/core';

/** The API has no poster images, so each movie gets a deterministic generated poster. */
@Component({
  selector: 'app-poster',
  template: `
    <div class="poster" [style.background]="bg()">
      <span class="genre">{{ genre() }}</span>
      <span class="title">{{ title() }}</span>
    </div>`,
  styles: [`
    .poster { aspect-ratio: 2/3; border-radius: 12px; padding: 14px; display: flex; flex-direction: column; justify-content: space-between; position: relative; overflow: hidden; color: #fff; }
    .poster::after { content: ''; position: absolute; inset: 0; background: radial-gradient(circle at 80% 10%, rgba(255,255,255,.22), transparent 45%); pointer-events: none; }
    .genre { font-size: 11px; font-weight: 600; letter-spacing: .12em; text-transform: uppercase; opacity: .85; }
    .title { font: 800 clamp(20px, 3vw, 28px)/1 var(--font-display); text-shadow: 0 2px 12px rgba(0,0,0,.35); overflow-wrap: anywhere; }
  `],
})
export class Poster {
  title = input.required<string>();
  genre = input<string>('');
  bg = computed(() => {
    let h = 0;
    for (const c of this.title()) h = (h * 31 + c.charCodeAt(0)) >>> 0;
    const a = h % 360, b = (a + 40 + (h >> 8) % 60) % 360;
    return `linear-gradient(155deg, hsl(${a} 62% 38%), hsl(${b} 70% 16%))`;
  });
}
