import { Service, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Product {
  id: number;
  name: string;
  description: string;
  price: number;
  stockQuantity: number;
  imageUrl?: string;
}



export interface ProductsResponse {
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  products: Product[];
}

@Service()
export class ProductService {

  private http = inject(HttpClient);

  private apiUrl = 'https://localhost:7187/api/products';

  getProducts(
    page: number = 1,
    pageSize: number = 10
  ): Observable<ProductsResponse> {
    return this.http.get<ProductsResponse>(
      `${this.apiUrl}?page=${page}&pageSize=${pageSize}`
    );
  }

  getProductById(id: number): Observable<Product> {
    return this.http.get<Product>(`${this.apiUrl}/${id}`);
  }
}
