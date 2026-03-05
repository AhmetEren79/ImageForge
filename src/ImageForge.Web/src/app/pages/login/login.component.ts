import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink, Router } from '@angular/router';
import { AuthService } from '../../core/auth.service';

@Component({
    selector: 'app-login',
    standalone: true,
    imports: [FormsModule, RouterLink],
    templateUrl: './login.html'
})
export class LoginComponent {
    emailOrUsername = '';
    password = '';
    error = signal('');
    loading = signal(false);

    constructor(private auth: AuthService, private router: Router) {
        if (auth.isLoggedIn()) this.router.navigate(['/studio']);
    }

    submit() {
        if (!this.emailOrUsername || !this.password) {
            this.error.set('Tüm alanları doldurun.');
            return;
        }

        this.loading.set(true);
        this.error.set('');

        this.auth.login({ emailOrUsername: this.emailOrUsername, password: this.password })
            .subscribe({
                next: () => this.router.navigate(['/studio']),
                error: (err) => {
                    this.loading.set(false);
                    this.error.set(err.error?.error || 'Giriş başarısız.');
                }
            });
    }
}
