import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const authService = inject(AuthService);

  return next(req).pipe(
    catchError((err: HttpErrorResponse) => {
      //Ignor 401s from login or refresh endpoints to avoid infinite loop
      if (err.status === 401 && !req.url.includes('/login') && !req.url.includes('/refresh')) {
        return authService.refreshToken().pipe(
          switchMap(() => next(req)), //retry original req with new cookie
          catchError((refreshErr) => {
            authService.clearSession();
            router.navigate(['/login']);
            return throwError(() => refreshErr);
          }),
        );
      }

      // Extract C# RFC 7807 ProblemDetails detail property
      const detailMessage = err.error?.detail ?? 'A system error occurred. Please try again.';

      // Surface structured error to developer console / UI notification
      console.error('API Error Response:', detailMessage);

      return throwError(() => err);
    }),
  );
};