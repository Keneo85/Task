import { HttpClient, HttpInterceptorFn } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { catchError, switchMap, tap, throwError } from 'rxjs';
import { environment } from '../environments/environment';

const API = `${environment.apiUrl}/auth`;

interface Tokens {
  accessToken: string;
  refreshToken: string;
}

// 1. SERVICIO: login/registro, refresh y logout
@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);

  isLoggedIn = signal(!!localStorage.getItem('token'));

  login(email: string, password: string, register = false) {
    return this.http
      .post<Tokens>(`${API}/${register ? 'register' : 'login'}`, { email, password })
      .pipe(tap((t) => this.save(t)));
  }

  refresh() {
    return this.http
      .post<Tokens>(`${API}/refresh`, { refreshToken: localStorage.getItem('refresh') })
      .pipe(tap((t) => this.save(t)));
  }

  logout() {
    localStorage.clear();
    this.isLoggedIn.set(false);
    this.router.navigate(['/login']);
  }

  private save(t: Tokens) {
    localStorage.setItem('token', t.accessToken);
    localStorage.setItem('refresh', t.refreshToken);
    this.isLoggedIn.set(true);
  }
}

// 2. INTERCEPTOR: agrega el token; si la API responde 401, hace refresh y reintenta
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  if (req.url.startsWith(API)) return next(req);

  const withToken = () =>
    req.clone({ setHeaders: { Authorization: `Bearer ${localStorage.getItem('token')}` } });

  return next(withToken()).pipe(
    catchError((err) => {
      if (err.status !== 401) return throwError(() => err);
      return auth.refresh().pipe(
        switchMap(() => next(withToken())),
        catchError((e) => {
          auth.logout();
          return throwError(() => e);
        }),
      );
    }),
  );
};

// 3. GUARD: sin sesión → /login
export const authGuard: CanActivateFn = () =>
  inject(AuthService).isLoggedIn() || inject(Router).createUrlTree(['/login']);
