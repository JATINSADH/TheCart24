import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { Home } from './components/home/home';
import { Products } from './components/products/products';

import { Login } from './components/login/login';

import { Register } from './components/register/register';

import { Admin } from './components/admin/admin';

import { adminGuard } from './guards/admin-guard';

import { SellerManagement } from './components/admin/seller-management/seller-management';

import { Cart } from './components/cart/cart';


import { CheckoutComponent } from './components/checkout-component/checkout-component';

import { ThankYouComponent } from './components/thank-you-component/thank-you-component';

import { authGuard } from './guards/auth-guard';

import { MyOrderComponent } from './components/my-order-component/my-order-component';

import { SellerDashboard } from './components/seller-dashboard/seller-dashboard';
const routes: Routes = [
  { path: '', component: Home },

  {
    path: 'admin',
    component: Admin,
    canActivate: [adminGuard]
  },
  {
    path: 'admin/sellers',
    component: SellerManagement,
    canActivate: [adminGuard]
  },
  {
    path: 'seller-dashboard',
    component: SellerDashboard,
    canActivate: [adminGuard]
  },
  {
    path: 'cart',
    component: Cart,
    canActivate: [authGuard]
  },
  {
    path: 'checkout',
    component: CheckoutComponent,
    canActivate: [authGuard]
  },
  {
    path: 'thank-you',
    component: ThankYouComponent,
    canActivate: [authGuard]
  },
  {
    path: 'my-orders',
    component: MyOrderComponent,
    canActivate: [authGuard]
  },

  { path: 'products', component: Products },
  { path: 'login', component: Login },
  { path: 'register', component: Register },

  { path: '**', redirectTo: '' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}
