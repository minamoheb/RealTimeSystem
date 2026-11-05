import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from './auth.service';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const auth = inject(AuthService);
  const skipAuth = request.url.includes('/auth/login') || request.url.includes('/auth/refresh');
  const token = auth.token();
  const outgoing = !skipAuth && token
    ? request.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : request;

  return next(outgoing).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status !== 401 || skipAuth || request.headers.has('X-Retry')) {
        return throwError(() => error);
      }

      return auth.refresh().pipe(
        switchMap(() => next(request.clone({
          setHeaders: {
            Authorization: `Bearer ${auth.token() ?? ''}`,
            'X-Retry': '1'
          }
        }))),
        catchError((refreshError) => {
          auth.logout();
          return throwError(() => refreshError);
        })
      );
    })
  );
};
