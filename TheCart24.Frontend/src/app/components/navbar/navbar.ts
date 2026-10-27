import { Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-navbar',
  standalone: false,
  styleUrl: './navbar.css',
  templateUrl: './navbar.html',
})
export class Navbar implements OnInit {

private router = inject(Router);
isLoggedIn = false;

ngOnInit(): void {
  this.checkLoginStatus();
}

checkLoginStatus(): void {
  this.isLoggedIn = !!sessionStorage.getItem('thecart24_token');
}

login(): void {
  this.router.navigate(['/login']);
}

logout(): void {
  sessionStorage.removeItem('thecart24_token');
  this.isLoggedIn = false;
  this.router.navigate(['/login']);
}



get isAdmin(): boolean {
  const token = sessionStorage.getItem('thecart24_token');

  if (!token) {
    return false;
  }

  try {
    const payload = JSON.parse(
      atob(token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/'))
    );

    const role =
      payload['role'] ??
      payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];

    return role === 'Admin' || role === 'SuperAdmin';
  } catch {
    return false;
  }
}

goToSellerPanel(): void {
  this.router.navigate(['/seller-dashboard']);
}


}
