import { Component, inject, ChangeDetectorRef} from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';

interface LoginResponse {
  token?: string;
  accessToken?: string;
  message?: string;
}

@Component({
  selector: 'app-login',
  standalone: false,
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {

  private http = inject(HttpClient);
  private router = inject(Router);
  private cdr = inject(ChangeDetectorRef);

  email = '';
  password = '';
  loading = false;
  errorMessage = '';

  
  login(): void {
    if (!this.email.trim() || !this.password) {
      this.errorMessage = 'Email aur password required hain.';
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    this.http.post<LoginResponse>(
      'https://localhost:7187/api/Auth/login',
      {
        email: this.email.trim(),
        password: this.password
      }
    ).subscribe({
      next: (response) => {
        const token = response.accessToken ?? response.token;

        if (!token) {
          this.errorMessage = 'Login response mein token nahi mili.';
          this.loading = false;
          return;
        }

        // Token browser session mein save karo
        sessionStorage.setItem('thecart24_token', token);

        try {
          // JWT payload decode karo
          const payloadPart = token.split('.')[1];
          const base64 = payloadPart
            .replace(/-/g, '+')
            .replace(/_/g, '/');

          const payload = JSON.parse(
            atob(base64.padEnd(
              base64.length + (4 - base64.length % 4) % 4,
              '='
            ))
          );

          console.log('JWT payload:', payload);

          // .NET ClaimTypes.Role ka claim
          const roleClaim =
            'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';

          const roleValue =
            payload[roleClaim] ?? payload['role'] ?? payload['roles'];

          const roles: string[] = Array.isArray(roleValue)
            ? roleValue
            : roleValue
              ? [roleValue]
              : [];

          const isAdmin = roles.some(
            role => role.toLowerCase() === 'admin'
          );

          this.loading = false;

          if (isAdmin) {
            console.log('Admin login successful');
            this.router.navigate(['/admin']);
          } else {
            console.log('Normal user login successful');
            this.router.navigate(['/home']);
          }
        } catch (error) {
          console.error('JWT decode error:', error);
          sessionStorage.removeItem('thecart24_token');
          this.errorMessage = 'Token read nahi ho paya.';
          this.loading = false;
        }
      },
      error: (error) => {
        console.error('Login error:', error);
        this.errorMessage =
          error.error?.message ?? 'Login failed. Credentials check karo.';
        this.loading = false;
      }
    });
  }

}
