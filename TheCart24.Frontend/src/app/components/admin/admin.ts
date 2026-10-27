
import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-admin',
  standalone: false,
  templateUrl: './admin.html',
  styleUrl: './admin.css'
})
export class Admin {
  private router = inject(Router);

 
openBuyers(): void {
  this.router.navigate(['/']);
}

openSellers(): void {
  this.router.navigate(['/seller-dashboard']);
}


logout(): void {
  sessionStorage.removeItem('thecart24_token');
  this.router.navigate(['/login']);
}
}
