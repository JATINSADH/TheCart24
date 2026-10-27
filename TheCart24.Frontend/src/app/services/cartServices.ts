
import { inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface CartItem {
  cartItemId: number;
  productId: number;
  productName: string;
  price: number;
  quantity: number;
  subtotal: number;
  imageUrl?: string;
}

export interface CartResponse {
  items: CartItem[];
  total: number;
}

export class CartServices {
  private http = inject(HttpClient);

  private readonly apiUrl = 'https://localhost:7187/api/Cart';

  private getHeaders(): HttpHeaders {
    const token = sessionStorage.getItem('thecart24_token');

    return new HttpHeaders({
      Authorization: `Bearer ${token ?? ''}`
    });
  }

  // Backend se cart fetch karna
  getCart(): Observable<CartResponse> {
    return this.http.get<CartResponse>(this.apiUrl, {
      headers: this.getHeaders()
    });
  }

  // Product backend cart mein add karna
  addToCart(
    productId: number,
    quantity: number = 1
  ): Observable<unknown> {
    return this.http.post(
      `${this.apiUrl}/items`,
      { productId, quantity },
      { headers: this.getHeaders() }
    );
  }



updateQuantity(cartItemId: number, quantity: number): Observable<unknown> {
  return this.http.put(
    `${this.apiUrl}/items/${cartItemId}`,
    quantity,
    { headers: this.getHeaders() }
  );
}

removeItem(cartItemId: number): Observable<unknown> {
  return this.http.delete(
    `${this.apiUrl}/items/${cartItemId}`,
    { headers: this.getHeaders() }
  );
}
}
