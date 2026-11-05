import { Component, input } from '@angular/core';

@Component({
  selector: 'app-status-pill',
  standalone: true,
  templateUrl: './status-pill.component.html',
  styleUrl: './status-pill.component.css'
})
export class StatusPillComponent {
  readonly status = input.required<string>();
}
