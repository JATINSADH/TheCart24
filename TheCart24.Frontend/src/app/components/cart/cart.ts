
import { Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { CartServices as CartService, CartItem } from '../../services/cartServices';

import { ChangeDetectorRef } from '@angular/core';

@Component({
  selector: 'app-cart',
  standalone: false,
  templateUrl: './cart.html',
  styleUrl: './cart.css'
})
export class Cart implements OnInit {
  private cartService = inject(CartService);
  private router = inject(Router);
  private cdr = inject(ChangeDetectorRef);

  cartItems: CartItem[] = [];
  total = 0;
  loading = true;
  errorMessage = '';

  ngOnInit(): void {
    this.loadCart();

    this.cdr.markForCheck();
  }

  loadCart(): void {
    this.loading = true;

    this.cartService.getCart().subscribe({
      next: (response) => {
        this.cartItems = response.items;
        this.total = response.total;
        this.loading = false;

        this.cdr.markForCheck();
      },
      error: (error) => {
        console.error('Cart load failed:', error);
        this.errorMessage = 'Cart not loded.';
        this.loading = false;

        if (error.status === 401) {
          this.router.navigate(['/login']);
        }
      }
    });
  }

get subtotal(): number {
  return this.cartItems.reduce(
    (total, item) => total + item.price * item.quantity,
    0
  );
}

increaseQuantity(item: CartItem): void {
  this.cartService.updateQuantity(
    item.cartItemId,
    item.quantity + 1
  ).subscribe({
    next: () => this.loadCart(),

    error: (error) => {
      console.error('Quantity increase failed:', error);
      alert(error.error?.message || 'Quantity not updaed.');
      this.cdr.markForCheck();
    }
  });
}

decreaseQuantity(item: CartItem): void {
  if (item.quantity <= 1) {
    this.removeItem(item.cartItemId);
    return;
  }

  this.cartService.updateQuantity(
    item.cartItemId,
    item.quantity - 1
  ).subscribe({
    next: () => this.loadCart(),
    error: (error) => {
      console.error('Quantity decrease failed:', error);
      alert(error.error?.message || 'Quantity not updated.');

this.cdr.markForCheck();
    }
  });
}

removeItem(cartItemId: number): void {
  if (!confirm('want to remove product')) {
    return;
  }

  this.cartService.removeItem(cartItemId).subscribe({
    next: () => this.loadCart(),
    error: (error) => {
      console.error('Remove item failed:', error);
      alert(error.error?.message || 'Product not removed.');

this.cdr.markForCheck();
    }
  });
}


goToProducts(): void {
  this.router.navigate(['/products']);
}



}
