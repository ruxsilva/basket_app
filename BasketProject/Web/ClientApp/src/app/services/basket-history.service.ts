import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {PaginatedResponse} from '../models/paginated-response.model';
import {BasketHistory} from '../models/basket-history.model';

@Injectable({
  providedIn: 'root'
})
export class BasketHistoryService {

  constructor(private http: HttpClient) {}

  getUserBasketHistory(page: number = 1, pageSize: number = 10): Observable<PaginatedResponse<BasketHistory>> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<PaginatedResponse<BasketHistory>>('api/basket/history', { params });
  }
}
