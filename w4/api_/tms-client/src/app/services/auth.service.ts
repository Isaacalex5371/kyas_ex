import { inject, Service, signal } from '@angular/core'; // 1. Use Injectable
import { HttpClient } from '@angular/common/http';        // 2. Correct HttpClient import
import { firstValueFrom } from 'rxjs';                    // 3. Correct rxjs import

export interface TmsUser {
  displayName: string;
  role: string;
}

export interface LoginRequest {
  username: string;
  password: string;
}

@Service()
export class AuthService {
  private http = inject(HttpClient);
  currentUser = signal<TmsUser | null>(null);

  hasRole(role: string): boolean {
    const user = this.currentUser();
    return user?.role === role || user?.role === 'Admin';
  }

  async login(credentials: LoginRequest) {
    // Note: Added 'credentials' as the body for the POST request
    await firstValueFrom(this.http.post<void>('/api/v2/auth/login', credentials));
    
    // Fetch authenticated profile
    const user = await firstValueFrom(this.http.get<TmsUser>('/api/v2/auth/me'));
    this.currentUser.set(user);
  }
}