import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { BasketItemModel } from '../models/basket-item.model';
import { ReceiptModel } from '../models/receipt.model';
import { ItemModel } from '../models/item.model';

@Injectable({
  providedIn: 'root'
})
export class BasketService {
  private apiUrl = 'api/basket';

  constructor(private http: HttpClient) { }

  getItems(): Observable<ItemModel[]> {
    return this.http.get<ItemModel[]>(`${this.apiUrl}/items`)
      .pipe(
        catchError(this.handleError<ItemModel[]>('getItems'))
      );
  }

  submitBasket(items: BasketItemModel[]): Observable<ReceiptModel> {
    const transformedItems = items.map(item => ({
      name: item.item.name,
      quantity: item.quantity
    }));

    return this.http.post<ReceiptModel>(`${this.apiUrl}/process`, { items: transformedItems })
      .pipe(
        catchError(this.handleError<ReceiptModel>('submitBasket'))
      );
  }

  private handleError<T>(operation = 'operation') {
    return (error: any): Observable<never> => {
      console.error(`${operation} failed: ${error.message}`);
      throw error;
    };
  }
}
