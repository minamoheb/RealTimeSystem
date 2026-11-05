import { Type } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { AlertDto, ServerDto } from '../core/models';
import { AlertListComponent, KpiCardComponent, ServerCardComponent, StatusPillComponent } from './widgets';

describe('StatusPillComponent', () => {
  it('renders the status text', () => {
    const fixture = create(StatusPillComponent);
    fixture.componentRef.setInput('status', 'Degraded');
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Degraded');
  });
});

describe('KpiCardComponent', () => {
  it('renders the label and value', () => {
    const fixture = create(KpiCardComponent);
    fixture.componentRef.setInput('label', 'Hosts');
    fixture.componentRef.setInput('value', 5);
    fixture.componentRef.setInput('hint', 'Active');
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Hosts');
    expect(fixture.nativeElement.textContent).toContain('5');
  });
});

describe('ServerCardComponent', () => {
  it('shows the host name and cpu', () => {
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      imports: [ServerCardComponent],
      providers: [provideRouter([])]
    });
    const fixture = TestBed.createComponent(ServerCardComponent);
    fixture.componentRef.setInput('server', server());
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('api-core-01');
    expect(fixture.nativeElement.textContent).toContain('72.0%');
  });
});

describe('AlertListComponent', () => {
  it('emits when an alert is acknowledged', () => {
    const fixture = create(AlertListComponent);
    fixture.componentRef.setInput('alerts', [alert()]);
    fixture.detectChanges();
    let emitted = 0;
    fixture.componentInstance.acknowledge.subscribe((id) => emitted = id);
    fixture.nativeElement.querySelector('button').click();
    expect(emitted).toBe(1);
  });
});

function create<T>(component: Type<T>): ComponentFixture<T> {
  TestBed.resetTestingModule();
  TestBed.configureTestingModule({ imports: [component] });
  return TestBed.createComponent(component);
}

function server(): ServerDto {
  return {
    id: 1,
    name: 'api-core-01',
    hostName: '10.0.1.21',
    environmentName: 'Production',
    description: null,
    status: 'Healthy',
    isActive: true,
    createdAtUtc: '2026-11-01T00:00:00Z',
    cpuPercent: 72,
    memoryPercent: 40,
    diskPercent: 30,
    networkMbps: 10,
    responseTimeMs: 80,
    collectedAtUtc: '2026-11-01T00:00:00Z',
    openAlertCount: 1
  };
}

function alert(): AlertDto {
  return {
    id: 1,
    serverId: 1,
    serverName: 'api-core-01',
    metricName: 'CPU',
    value: 91,
    threshold: 90,
    severity: 'Critical',
    message: 'CPU is high',
    isAcknowledged: false,
    createdAtUtc: '2026-11-01T00:00:00Z',
    acknowledgedAtUtc: null
  };
}
