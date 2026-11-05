import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { LoginPage } from './login.page';

describe('LoginPage', () => {
  let fixture: ComponentFixture<LoginPage>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [LoginPage],
      providers: [provideRouter([]), provideHttpClient()]
    });
    fixture = TestBed.createComponent(LoginPage);
    fixture.detectChanges();
  });

  it('shows a validation message for a short password', () => {
    fixture.componentInstance.form.controls.password.setValue('short');
    fixture.componentInstance.form.controls.password.markAsTouched();
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('at least 8 characters');
  });

  it('keeps the submit disabled while a sign-in is in progress', () => {
    fixture.componentInstance.busy.set(true);
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('button').disabled).toBeTrue();
    expect(fixture.nativeElement.textContent).toContain('Signing in');
  });
});
