
import { Component, inject, OnInit, ChangeDetectorRef
  } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Router } from '@angular/router';
import { CartServices as CartService, CartItem, CartResponse } from '../../services/cartServices';

import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

interface CheckoutResponse {
  message?: string;
  orderId?: number;
  [key: string]: unknown;
}

interface ProductDiscountResponse {
  id?: number;
  discountPercentage?: number;
  discountAmount?: number;
  discountedPrice?: number;
  discountName?: string;
  message?: string;
}

@Component({
  selector: 'app-checkout',
  standalone: false,
  templateUrl: './checkout-component.html',
  styleUrl: './checkout-component.css'
})
export class CheckoutComponent implements OnInit {
  private http = inject(HttpClient);
  private router = inject(Router);
  private cartService = inject(CartService);
  private cdr = inject(ChangeDetectorRef);

  private readonly apiUrl = 'https://localhost:7187/api/Orders/checkout';

  customerName = '';
  mobileNumber = '';
  address = '';
  city = '';
  state = '';
  pincode = '';

  paymentMethod = 'COD';
  couponCode = '';

  cartItems: CartItem[] = [];
  loadingCart = false;
  cartError = '';

  loading = false;
  errorMessage = '';

  ngOnInit(): void {
    this.loadCartSummary();

this.loadActiveOffer();
  }

  loadCartSummary(): void {
    this.loadingCart = true;
    this.cartError = '';

    this.cartService.getCart().subscribe({
      next: (response: CartResponse) => {
        this.cartItems = response.items ?? [];
        this.loadingCart = false;

        this.cdr.markForCheck();      
      },
      error: (error) => {
        this.loadingCart = false;
        console.error('Failed to load cart:', error);
        this.cartError = 'Could not load your cart. Please try again.';

this.cdr.markForCheck();
      }
    });
  }

  get subtotal(): number {
    return this.cartItems.reduce(
      (sum, item) => sum + item.price * item.quantity,
      0
    );
  }

  get deliveryCharge(): number {
    return 0; // Free delivery
  }

  get totalAmount(): number {
    return Math.max(
      0,
      this.subtotal - this.discount + this.deliveryCharge
    );
  }

 private readonly discountApiUrl =
  'https://localhost:7187/api/Discounts/apply';

appliedDiscountPercentage = 0;
activeDiscountName = '';

applyingDiscount = false;
discountApplied = false;
discountMessage = '';

get discount(): number {
  if (!this.discountApplied) {
    return 0;
  }

  return Math.round(
    (this.subtotal * this.appliedDiscountPercentage / 100 + Number.EPSILON) * 100
  ) / 100;
}

applyCoupon(): void {
  if (this.cartItems.length === 0) {
    this.discountMessage = 'Your cart is empty.';
    return;
  }

  const productIds = [
    ...new Set(this.cartItems.map(item => item.productId))
  ];

  this.applyingDiscount = true;
  this.discountApplied = false;
  this.appliedDiscountPercentage = 0;
  this.activeDiscountName = '';
  this.discountMessage = '';

  const requests = productIds.map(productId =>
    this.http.post<ProductDiscountResponse>(
      this.discountApiUrl,
      { productId }
    ).pipe(
      catchError(error => {
        console.error(
          `Discount lookup failed for product ${productId}:`,
          error
        );
        return of(null);
      })
    )
  );

  forkJoin(requests).subscribe({
    next: responses => {
      const discounts = responses
        .filter((response): response is ProductDiscountResponse =>
          response !== null &&
          (response.discountPercentage ?? 0) > 0
        )
        .sort(
          (a, b) =>
            (b.discountPercentage ?? 0) -
            (a.discountPercentage ?? 0)
        );

      const bestDiscount = discounts[0];

      if (!bestDiscount) {
        this.discountMessage =
          'No active discount is available for your cart.';
        this.applyingDiscount = false;
        this.cdr.markForCheck();
        return;
      }

      this.appliedDiscountPercentage =
        bestDiscount.discountPercentage ?? 0;

      this.activeDiscountName =
        bestDiscount.discountName ?? 'Special Offer';

      this.discountApplied = true;

      this.cdr.markForCheck();

      this.discountMessage =
        `${this.activeDiscountName}: ${this.appliedDiscountPercentage}% discount found.`;

      this.applyingDiscount = false;
      this.cdr.markForCheck();
    },
    error: error => {
      console.error('Discount API failed:', error);
      this.discountMessage =
        'Could not check discounts. Please try again.';
      this.applyingDiscount = false;
      this.cdr.markForCheck();
    }
  });
}

  placeOrder(): void {
    const token = sessionStorage.getItem('thecart24_token');

    if (!token) {
      this.router.navigate(['/login']);
      return;
    }

    if (this.cartItems.length === 0) {
      this.errorMessage = 'Your cart is empty. Please add products first.';
      return;
    }

    this.errorMessage = '';

    if (
      !this.customerName.trim() ||
      !this.mobileNumber.trim() ||
      !this.address.trim() ||
      !this.city.trim() ||
      !this.state.trim() ||
      !this.pincode.trim()
    ) {
      this.errorMessage = 'Please fill in all delivery address fields.';
      return;
    }

    const body = {
      fullName: this.customerName.trim(),
      phone: this.mobileNumber.trim(),
      addressLine: this.address.trim(),
      city: this.city.trim(),
      state: this.state.trim(),
      postalCode: this.pincode.trim(),
      paymentMethod: this.paymentMethod,

      applyDiscount: this.discountApplied
    };

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`,
      'Idempotency-Key': crypto.randomUUID()
    });

    this.loading = true;
    this.errorMessage = '';

    this.http.post<CheckoutResponse>(this.apiUrl, body, { headers }).subscribe({
      next: (response) => {
        this.loading = false;
        console.log('Order checkout response:', response);
        this.cdr.markForCheck();
        this.router.navigate(['/thank-you'], {
          state: { order: response }
        });
      },
      error: (error) => {
        this.loading = false;
        console.error('Checkout failed:', error);

        this.errorMessage =
          error.error?.message ||
          'Checkout failed. Please check your details and try again.';

          this.cdr.markForCheck();
      }

             
    });
  }






activeOffer: {
  name: string;
  discountPercentage: number;
} | null = null;

loadActiveOffer(): void {
  this.http.get<{
    name: string;
    discountPercentage: number;
  }>('https://localhost:7187/api/Discounts/announcement')
    .subscribe({
      next: offer => {
        this.activeOffer = offer;
        this.cdr.markForCheck();
      },
      error: error => {
        console.error('Failed to load offer:', error);
        this.activeOffer = null;
        this.cdr.markForCheck();
      }
    });
}



}
