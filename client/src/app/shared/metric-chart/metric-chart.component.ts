import { AfterViewInit, Component, ElementRef, effect, input, viewChild } from '@angular/core';
import { Chart, ChartConfiguration, registerables } from 'chart.js';

Chart.register(...registerables);

export interface ChartSeries {
  label: string;
  data: number[];
  color: string;
}

@Component({
  selector: 'app-metric-chart',
  standalone: true,
  templateUrl: './metric-chart.component.html',
  styleUrl: './metric-chart.component.css'
})
export class MetricChartComponent implements AfterViewInit {
  readonly labels = input<string[]>([]);
  readonly series = input<ChartSeries[]>([]);
  readonly kind = input<'line' | 'bar'>('line');
  private readonly canvas = viewChild<ElementRef<HTMLCanvasElement>>('canvas');
  private chart?: Chart;

  constructor() {
    effect(() => {
      this.labels();
      this.series();
      this.kind();
      this.draw();
    });
  }

  ngAfterViewInit(): void {
    this.draw();
  }

  private draw(): void {
    const element = this.canvas()?.nativeElement;
    if (!element) {
      return;
    }

    const config: ChartConfiguration = {
      type: this.kind(),
      data: {
        labels: this.labels(),
        datasets: this.series().map((series) => ({
          label: series.label,
          data: series.data,
          borderColor: series.color,
          backgroundColor: this.kind() === 'bar' ? series.color : `${series.color}33`,
          tension: 0.35,
          fill: this.kind() === 'line',
          pointRadius: 0,
          borderWidth: 2
        }))
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { labels: { color: '#9aa892', boxWidth: 12 } } },
        scales: {
          x: { ticks: { color: '#9aa892', maxTicksLimit: 6 }, grid: { color: 'rgba(255,255,255,.05)' } },
          y: { ticks: { color: '#9aa892' }, grid: { color: 'rgba(255,255,255,.05)' }, beginAtZero: true }
        }
      }
    };

    if (this.chart) {
      this.chart.data = config.data;
      this.chart.update();
      return;
    }

    this.chart = new Chart(element, config);
  }
}
