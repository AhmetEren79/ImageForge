import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs';
import { environment } from './environment';
import { AuthResponse, LoginRequest, RegisterRequest } from './models';

@Injectable({ providedIn: 'root' })
export class AuthService {
    private readonly TOKEN_KEY = 'imageforge_token';
    private readonly USER_KEY = 'imageforge_user';

    currentUser = signal<AuthResponse | null>(this.loadUser());
    isLoggedIn = computed(() => !!this.currentUser());

    constructor(private http: HttpClient, private router: Router) { }

    login(request: LoginRequest) {
        return this.http.post<AuthResponse>(`${environment.apiUrl}/auth/login`, request)
            .pipe(tap(res => this.setSession(res)));
    }

    register(request: RegisterRequest) {
        return this.http.post<AuthResponse>(`${environment.apiUrl}/auth/register`, request)
            .pipe(tap(res => this.setSession(res)));
    }

    logout() {
        localStorage.removeItem(this.TOKEN_KEY);
        localStorage.removeItem(this.USER_KEY);
        this.currentUser.set(null);
        this.router.navigate(['/login']);
    }

    getToken(): string | null {
        return localStorage.getItem(this.TOKEN_KEY);
    }

    private setSession(res: AuthResponse) {
        localStorage.setItem(this.TOKEN_KEY, res.token);
        localStorage.setItem(this.USER_KEY, JSON.stringify(res));
        this.currentUser.set(res);
    }

    private loadUser(): AuthResponse | null {
        try {
            const data = localStorage.getItem(this.USER_KEY);
            return data ? JSON.parse(data) : null;
        } catch {
            return null;
        }
    }
}
