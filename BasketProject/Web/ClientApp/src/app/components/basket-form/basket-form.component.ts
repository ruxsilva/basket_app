import { Component, OnInit } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { BasketService } from '../../services/basket.service';
import { BasketItemModel } from '../../models/basket-item.model';
import { ReceiptModel } from '../../models/receipt.model';
import { ItemModel } from '../../models/item.model';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ReceiptDisplayComponent } from '../receipt-display/receipt-display.component';
import { MatCardModule } from '@angular/material/card';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatListModule } from '@angular/material/list';
import { MatDividerModule } from '@angular/material/divider';
import { MatError } from '@angular/material/input';

@Component({
  selector: 'app-basket-form',
  templateUrl: './basket-form.component.html',
  styleUrls: ['./basket-form.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatToolbarModule,
    MatIconModule,
    MatError,
    MatProgressSpinnerModule,
    MatListModule,
    MatDividerModule,
    ReceiptDisplayComponent
  ]
})
export class BasketFormComponent implements OnInit {
  basketForm: FormGroup;
  errorMessage = '';
  loading = false;
  loadingItems = false;
  receipt: ReceiptModel | null = null;
  availableItems: ItemModel[] = [];

  itemQuantities: Map<string, number> = new Map();

  constructor(
    private fb: FormBuilder,
    private basketService: BasketService
  ) {
    this.basketForm = this.fb.group({});
  }

  ngOnInit(): void {
    this.loadingItems = true;

    this.basketService.getItems().pipe(
      catchError(error => {
        console.warn('Failed to load items from API', error);
        return throwError(() => error);
      })
    ).subscribe({
      next: (items) => {
        this.availableItems = items;
        this.loadingItems = false;

        this.availableItems.forEach(item => {
          this.itemQuantities.set(item.name, 0);
        });
      },
      error: (error) => {
        this.errorMessage = 'Failed to load available items';
        this.loadingItems = false;
      }
    });
  }

  getItemQuantity(item: ItemModel): number {
    return this.itemQuantities.get(item.name) || 0;
  }

  incrementItemQuantity(item: ItemModel): void {
    const currentQuantity = this.getItemQuantity(item);
    this.itemQuantities.set(item.name, currentQuantity + 1);
  }

  decrementItemQuantity(item: ItemModel): void {
    const currentQuantity = this.getItemQuantity(item);
    if (currentQuantity > 0) {
      this.itemQuantities.set(item.name, currentQuantity - 1);
    }
  }

  getSelectedItems(): ItemModel[] {
    return this.availableItems.filter(item => this.getItemQuantity(item) > 0);
  }

  getBasketTotal(): number {
    return this.availableItems.reduce((sum, item) => {
      return sum + (item.price * this.getItemQuantity(item));
    }, 0);
  }

  hasItemsInBasket(): boolean {
    return this.getBasketTotal() > 0;
  }

  submitBasket(): void {
    if (!this.hasItemsInBasket()) {
      this.errorMessage = 'Please add at least one item to your basket';
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    const items: BasketItemModel[] = this.getSelectedItems().map(item => ({
      item: {
        name: item.name,
        price: item.price
      },
      quantity: this.getItemQuantity(item),
      lineTotal: item.price * this.getItemQuantity(item)
    }));

    this.basketService.submitBasket(items).subscribe({
      next: (receipt) => {
        this.receipt = receipt;
        this.loading = false;
        this.availableItems.forEach(item => {
          this.itemQuantities.set(item.name, 0);
        });
      },
      error: (error) => {
        this.errorMessage = error.message || 'Failed to process basket';
        this.loading = false;
      }
    });
  }

  updateItemQuantity(item: any, event: Event): void {
    const input = event.target as HTMLInputElement;
    const newQuantity = parseInt(input.value, 10);

    if (isNaN(newQuantity) || newQuantity < 0) {
      input.value = this.getItemQuantity(item).toString();
      return;
    }

    this.itemQuantities.set(item.name, newQuantity);
  }
}
