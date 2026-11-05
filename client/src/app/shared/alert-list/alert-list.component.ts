import { Component, input, output } from '@angular/core';
import { AlertDto } from '../../core/models';

@Component({
  selector: 'app-alert-list',
  standalone: true,
  templateUrl: './alert-list.component.html',
  styleUrl: './alert-list.component.css'
})
export class AlertListComponent {
  readonly alerts = input.required<AlertDto[]>();
  readonly acknowledge = output<number>();
}
