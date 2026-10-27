import { HttpClient } from '@angular/common/http';
import { Component, OnInit, inject, ChangeDetectorRef
 } from '@angular/core';

@Component({
  selector: 'app-home',
  standalone: false,
  styleUrl: './home.css',
  templateUrl: './home.html',
})
export class Home implements OnInit {
  private http = inject(HttpClient);
  private cdr = inject(ChangeDetectorRef
);
  ngOnInit(): void {
  this.loadActiveOffer();
}
activeOffer: {
  name: string;
  discountPercentage: number;
} | null = null;

loadActiveOffer(): void {
  this.http.get<{
    name: string;
    discountPercentage: number;
  }>('https://localhost:7187/api/Discounts/announcement')
    .subscribe({
      next: offer => {
        this.activeOffer = offer;

        this.cdr.markForCheck();
      },
      error: error => {
        console.error('Failed to load active offer:', error);
        this.activeOffer = null;

this.cdr.markForCheck();
      }
    });
}
}
