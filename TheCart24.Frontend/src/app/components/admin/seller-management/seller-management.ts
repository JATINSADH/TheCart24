
import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-seller-management',
  standalone: false,
  templateUrl: './seller-management.html',
  styleUrl: './seller-management.css'
})
export class SellerManagement {
  private router = inject(Router);

  goHome(): void {
    this.router.navigate(['/']);
  }
}
