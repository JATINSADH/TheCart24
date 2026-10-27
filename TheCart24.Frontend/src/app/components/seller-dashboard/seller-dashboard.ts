
import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';

interface SellerDashboardData {
  totalProducts: number;
  totalOrders: number;
  pendingOrders: number;
  shippedOrders: number;
  deliveredOrders: number;
  totalSales: number;
}

@Component({
  selector: 'app-seller-dashboard',
  standalone: false,
  templateUrl: './seller-dashboard.html',
  styleUrl: './seller-dashboard.css'
})
export class SellerDashboard implements OnInit {
  private http = inject(HttpClient);
  private cdr = inject(ChangeDetectorRef);

  dashboard: SellerDashboardData = {
    totalProducts: 0,
    totalOrders: 0,
    pendingOrders: 0,
    shippedOrders: 0,
    deliveredOrders: 0,
    totalSales: 0
  };

  loading = true;
  errorMessage = '';

  ngOnInit(): void {
    this.loadDashboard();
  }

  loadDashboard(): void {
    const token = sessionStorage.getItem('thecart24_token');

    if (!token) {
      this.errorMessage = 'Please log in to access the dashboard.';
      this.loading = false;
      return;
    }

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    this.http.get<SellerDashboardData>(
      'https://localhost:7187/api/seller-dashboard',
      { headers }
    ).subscribe({
      next: data => {
        this.dashboard = data;
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: error => {
        console.error('Seller dashboard failed:', error);
        this.errorMessage = 'Dashboard load nahi hua. Please try again.';
        this.loading = false;
        this.cdr.markForCheck();
      }
    });
  }

  refreshDashboard(): void {
    this.loading = true;
    this.errorMessage = '';
    this.loadDashboard();
  }
}
