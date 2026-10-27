
import { Component, OnInit, ChangeDetectorRef, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Router } from '@angular/router';

interface AccountDetails {
  fullName: string;
  email: string;
  phoneNumber: string;
}

@Component({
  selector: 'app-my-account',
  standalone: false,
  templateUrl: './my-account.html',
  styleUrl: './my-account.css'
})
export class MyAccount implements OnInit {
  private http = inject(HttpClient);
  private router = inject(Router);
  private cdr = inject(ChangeDetectorRef);

  account: AccountDetails = {
    fullName: '',
    email: '',
    phoneNumber: ''
  };

  loading = true;
  deleting = false;
  errorMessage = '';
  successMessage = '';

  private readonly apiUrl = 'https://localhost:7187/api/Account';

  ngOnInit(): void {
    this.loadAccount();
  }

  private getHeaders(): HttpHeaders {
    return new HttpHeaders({
      Authorization: `Bearer ${sessionStorage.getItem('thecart24_token')}`
    });
  }

  loadAccount(): void {
    this.http.get<AccountDetails>(`${this.apiUrl}/me`, {
      headers: this.getHeaders()
    }).subscribe({
      next: data => {
        this.account = data;
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: error => {
        console.error(error);
        this.errorMessage = 'Could not load account details.';
        this.loading = false;
        this.cdr.markForCheck();
      }
    });
  }

  deleteAccount(): void {
    if (!confirm('Are you sure you want to permanently delete your account? This action cannot be undone.')) {
      return;
    }

    this.deleting = true;
    this.errorMessage = '';

    this.http.delete(`${this.apiUrl}/me`, {
      headers: this.getHeaders()
    }).subscribe({
      next: () => {
        sessionStorage.removeItem('thecart24_token');
        this.deleting = false;
        this.cdr.markForCheck();
        this.router.navigate(['/']);
      },
      error: error => {
        console.error(error);
        this.errorMessage =
          error.error?.message || 'Account could not be deleted.';
        this.deleting = false;
        this.cdr.markForCheck();
      }
    });
  }

  goBack(): void {
  if (window.history.length > 1) {
    window.history.back();
  } else {
    this.router.navigate(['/']);
  }
}
}
