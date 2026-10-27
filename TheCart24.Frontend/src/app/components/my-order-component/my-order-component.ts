
import { Component, inject, OnInit, ChangeDetectorRef
 } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Router } from '@angular/router';

interface OrderItem {
  productId: number;
  productName: string;
  unitPrice: number;
  quantity: number;
  subtotal: number;
  sellerStatus?: string;
}

interface Order {
  orderId: number;
  orderDate: string;
  totalAmount: number;
  status: string;
  shippingFullName?: string;
  shippingPhone?: string;
  shippingAddressLine?: string;
  shippingCity?: string;
  shippingState?: string;
  shippingPostalCode?: string;
  paymentMethod?: string;
  paymentStatus?: string;
  items: OrderItem[];
}

@Component({
  selector: 'app-my-order-component',
  standalone: false,
  templateUrl: './my-order-component.html',
  styleUrl: './my-order-component.css'
})
export class MyOrderComponent implements OnInit {
  private http = inject(HttpClient);
  private router = inject(Router);
  private cdr = inject(ChangeDetectorRef);

  private readonly apiUrl = 'https://localhost:7187/api/Orders/my';

  orders: Order[] = [];
  loading = false;
  errorMessage = '';

  ngOnInit(): void {
    this.loadOrders();
  }

  loadOrders(): void {
    const token = sessionStorage.getItem('thecart24_token');

    if (!token) {
      this.router.navigate(['/login'], {
        queryParams: { returnUrl: '/my-orders' }
      });
      return;
    }

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    this.loading = true;
    this.errorMessage = '';

    this.http.get<Order[]>(this.apiUrl, { headers }).subscribe({
      next: (response) => {
        this.orders = response;
        this.loading = false;

        this.cdr.markForCheck();
      },
      error: (error) => {
        this.loading = false;
        console.error('Failed to load orders:', error);

        if (error.status === 401) {
          sessionStorage.removeItem('thecart24_token');
          this.router.navigate(['/login'], {
            queryParams: { returnUrl: '/my-orders' }
          });

              this.cdr.markForCheck();
          return;
        }

        if (error.status === 403) {
          this.errorMessage =
            'You do not have permission to view these orders.';
          return;
        }

        this.errorMessage =
          'Could not load your orders. Please try again.';
      }
    });
  }

  getItemCount(order: Order): number {
    return order.items.reduce(
      (total, item) => total + item.quantity,
      0
    );
  }

  getStatusClass(status: string): string {
    return status.toLowerCase().replace(/\s+/g, '-');
  }
}
