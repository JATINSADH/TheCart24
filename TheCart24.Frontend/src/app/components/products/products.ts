import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { ProductService, Product } from '../../services/product';

import { CartServices } from '../../services/cartServices';

import { Router } from '@angular/router';



@Component({
  selector: 'app-products',
  standalone: false,
  templateUrl: './products.html',
  styleUrl: './products.css'
})
export class Products implements OnInit {

  products: Product[] = [];
  loading = true;
  errorMessage = '';


  page = 1;
pageSize = 10;
totalCount = 0;
totalPages = 0;


  private productService = inject(ProductService);
  private cdr = inject(ChangeDetectorRef);
  private router = inject(Router);  
  private cart = inject(CartServices);



ngOnInit(): void {
  this.loadProducts();
}


  loadProducts(): void {
  this.loading = true;
  this.errorMessage = '';

  this.productService.getProducts(this.page, this.pageSize)
    .subscribe({
     next: (data) => {
        console.log('Full response:', data);
        console.log('Products array:', data.products);

        this.products = data.products;
        console.log('Assigned products:', this.products);


      this.totalCount = data.totalCount;
      this.totalPages = data.totalPages;
      this.page = data.page;
      this.pageSize = data.pageSize;
      this.loading = false;

      this.cdr.markForCheck();;// Ensure change detection runs after async data is loaded
      },
      error: (error) => {
        console.error('HTTP status:', error.status);
        console.error('Error message:', error.message);
        console.error('Full error:', error);

        this.errorMessage =
          `not laoded. Status: ${error.status}`;

        this.loading = false;

        this.cdr.markForCheck();
      }
    });
}



goToPage(pageNumber: number): void {
  if (
    pageNumber < 1 ||
    pageNumber > this.totalPages ||
    pageNumber === this.page
  ) {
    return;
  }

  this.page = pageNumber;
  this.loadProducts();
}




addToCart(product: Product): void {
  if (!sessionStorage.getItem('thecart24_token')) {
    this.router.navigate(['/login']);
    return;
  }

  this.cart.addToCart(product.id, 1).subscribe({
    next: () => {
      console.log('added to cart.');
      this.router.navigate(['/cart']);
    },
    error: (error) => {
      console.error('Add to cart failed:', error);
      alert('add to cart failed.');
    }
  });
}



goHome(): void {
  this.router.navigate(['/']);
}

goToCart(): void {
  this.router.navigate(['/cart']);
}

}
