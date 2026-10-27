
import { Component } from '@angular/core';
import { Router } from '@angular/router';

interface OrderDetails {
  orderId?: number;
  totalAmount?: number;
  status?: string;
}

@Component({
  selector: 'app-thank-you',
  standalone: false,
  templateUrl: './thank-you-component.html',
  styleUrl: './thank-you-component.css'
})
export class ThankYouComponent {
  order: OrderDetails | null =
    history.state?.order ?? null;
}
