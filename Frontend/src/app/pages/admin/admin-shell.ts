import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-admin-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <h1 style="margin-bottom:20px">Admin</h1>
    <nav class="row sub" aria-label="Admin sections">
      @for (l of links; track l[0]) { <a class="chip" [routerLink]="l[0]" routerLinkActive="on">{{ l[1] }}</a> }
    </nav>
    <router-outlet />
  `,
  styles: [`.sub { margin-bottom: 24px; } a.chip { display: inline-block; }`],
})
export class AdminShell {
  links = [['movies', 'Movies'], ['theatres', 'Theatres'], ['screens', 'Screens & seats'], ['shows', 'Shows'], ['bookings', 'Bookings'], ['users', 'Add admin']];
}
