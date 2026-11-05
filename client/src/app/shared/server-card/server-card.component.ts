import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { formatNumber, formatPercent, toneFor } from '../../core/format';
import { ServerDto } from '../../core/models';
import { StatusPillComponent } from '../status-pill/status-pill.component';

@Component({
  selector: 'app-server-card',
  standalone: true,
  imports: [RouterLink, StatusPillComponent],
  templateUrl: './server-card.component.html',
  styleUrl: './server-card.component.css'
})
export class ServerCardComponent {
  readonly server = input.required<ServerDto>();
  readonly percent = formatPercent;
  readonly number = formatNumber;
  readonly tone = toneFor;
}
