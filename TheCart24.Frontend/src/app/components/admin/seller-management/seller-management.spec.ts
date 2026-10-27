import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SellerManagement } from './seller-management';

describe('SellerManagement', () => {
  let component: SellerManagement;
  let fixture: ComponentFixture<SellerManagement>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [SellerManagement],
    }).compileComponents();

    fixture = TestBed.createComponent(SellerManagement);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
