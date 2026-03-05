import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink, Router } from '@angular/router';
import { AuthService } from '../../core/auth.service';

@Component({
    selector: 'app-register',
    standalone: true,
    imports: [FormsModule, RouterLink],
    templateUrl: './register.html'
})
export class RegisterComponent {
    email = '';
    username = '';
    password = '';
    displayName = '';
    error = signal('');
    loading = signal(false);

    constructor(private auth: AuthService, private router: Router) {
        if (auth.isLoggedIn()) this.router.navigate(['/studio']);
    }

    submit() {
        if (!this.email || !this.username || !this.password) {
            this.error.set('E-posta, kullanıcı adı ve şifre zorunludur.');
            return;
        }

        this.loading.set(true);
        this.error.set('');

        this.auth.register({
            email: this.email,
            username: this.username,
            password: this.password,
            displayName: this.displayName || undefined
        }).subscribe({
            next: () => this.router.navigate(['/studio']),
            error: (err) => {
                this.loading.set(false);
                this.error.set(err.error?.error || 'Kayıt başarısız.');
            }
        });
    }
}
