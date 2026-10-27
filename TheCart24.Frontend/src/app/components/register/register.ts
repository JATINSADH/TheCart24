
import { Component, inject, ChangeDetectorRef
  } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';

interface RegisterResponse {
  message?: string;
}

@Component({
  selector: 'app-register',
  standalone: false,
  templateUrl: './register.html',
  styleUrl: './register.css'
})
export class Register {

  private http = inject(HttpClient);
  private router = inject(Router);
  private cdr = inject(ChangeDetectorRef);

  name = '';
  email = '';
  password = '';
  confirmPassword = '';

  loading = false;
  errorMessage = '';
  successMessage = '';

  register(): void {
    this.errorMessage = '';
    this.successMessage = '';

    if (
      !this.name.trim() ||
      !this.email.trim() ||
      !this.password ||
      !this.confirmPassword
    ) {
      this.errorMessage = 'Please fill in all fields.';
      return;
    }

    if (this.password !== this.confirmPassword) {
      this.errorMessage = 'Passwords do not match.';
      return;
    }

    this.loading = true;

    this.http.post<RegisterResponse>(
      'https://localhost:7187/api/Auth/register',
      {
        userName: this.name.trim(),
        email: this.email.trim(),
        password: this.password
      }
    ).subscribe({
      next: (response) => {
        this.loading = false;
        this.successMessage =
          response.message || 'Registration successful! Please log in.';
                  this.cdr.markForCheck(); // Trigger change detection to update the view
           this.router.navigate(['/login']);
      },
      error: (error) => {
        console.error('Registration error:', error);
        this.loading = false;
        this.errorMessage =
          error.error?.message ||
          error.error?.title ||
          'Registration failed. Please check your details.';
      }
    });
  }
}
