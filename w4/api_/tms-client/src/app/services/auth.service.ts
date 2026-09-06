import { HttpClient } from '@angular/common/http';
import { inject, Injectable, Service, signal } from '@angular/core';
import { Router } from '@angular/router';
import { firstValueFrom, Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';

export interface TmsUser {
  userId: string;
  email: string;
  displayName: string;
  role: string;
}
export interface LoginRequest {
  email: string;
  password: string;
}
export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  role: string;
}

@Service()
export class AuthService {
  private http = inject(HttpClient);
  private readonly authUrl = `${environment.apiUrl}/v1/auth`;
  private router = inject(Router);

  //public accessToken = signal<string | null>(null);
  currentUser = signal<TmsUser | null>(null);

  // getAccessToken(): string | null {
  //   return this.accessToken();
  // }

  hasRole(role: string): boolean {
    const user = this.currentUser();
    return user?.role === role;
  }

async login(credentials: LoginRequest): Promise<void> {
  const user = await firstValueFrom(
    this.http.post<TmsUser>(`${this.authUrl}/login`, credentials),
  );

  this.currentUser.set(user);

  if (user.role.toLowerCase() === 'instructor') {
    await this.router.navigate(['/i-dashboard']);
    return;
  }

  await this.router.navigate(['/dashboard']);
}

  //called once on app startup
  async checkSession(): Promise<void> {
    try {
      const user = await firstValueFrom(this.http.get<TmsUser>(`${this.authUrl}/me`));
      this.currentUser.set(user);
    } catch {
      this.currentUser.set(null);
    }
  }

  refreshToken(): Observable<void> {
    return this.http.post<void>(`${this.authUrl}/refresh`, {});
  }

  async register(data: RegisterRequest): Promise<void> {
    await firstValueFrom(this.http.post('/api/v2/auth/register', data));
    this.router.navigate(['/login']);
  }

  async logOut(): Promise<void> {
    try {
      await firstValueFrom(this.http.post(`${this.authUrl}/logout`, {}));
    } finally {
      this.clearSession();
      this.router.navigate(["/login"])
    }
  }

  clearSession(): void {
    this.currentUser.set(null);
  }
}