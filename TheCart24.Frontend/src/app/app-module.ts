import { NgModule, provideBrowserGlobalErrorListeners } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import { FormsModule } from '@angular/forms';

import { AppRoutingModule } from './app-routing-module';
import { App } from './app';
import { Home } from './components/home/home';
import { Navbar } from './components/navbar/navbar';
import { Products } from './components/products/products';
import { Login } from './components/login/login';
import { Register } from './components/register/register';
import { Admin } from './components/admin/admin';
import { SellerManagement } from './components/admin/seller-management/seller-management';

import { CartServices } from './services/cartServices';

import { Cart } from './components/cart/cart';
import { CheckoutComponent } from './components/checkout-component/checkout-component';
import { ThankYouComponent } from './components/thank-you-component/thank-you-component';
import { MyOrderComponent } from './components/my-order-component/my-order-component';
import { SellerDashboard } from './components/seller-dashboard/seller-dashboard';

@NgModule({
  declarations: [
    App,
    Home,
    Navbar,
    Products,
    Login,
    Register,
    Admin,
    SellerManagement,
    Cart,
    CheckoutComponent,
    ThankYouComponent,
    MyOrderComponent,
    SellerDashboard,
  ],
  imports: [BrowserModule, AppRoutingModule, CommonModule, HttpClientModule, FormsModule],
  providers: [provideBrowserGlobalErrorListeners(), CartServices],
  bootstrap: [App],
})
export class AppModule {}
