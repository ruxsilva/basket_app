import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { BasketHistoryService } from '../../services/basket-history.service';
import { BasketHistory } from '../../models/basket-history.model';

@Component({
  selector: 'app-basket-history',
  templateUrl: './basket-history.component.html',
  styleUrls: ['./basket-history.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatPaginatorModule,
    MatSelectModule,
    MatFormFieldModule
  ]
})
export class BasketHistoryComponent implements OnInit {
  basketHistory: BasketHistory[] = [];
  pagination = {
    pageNumber: 1,
    pageSize: 10,
    totalCount: 0,
    totalPages: 0
  };

  loading = false;
  error: string | null = null;

  constructor(private basketHistoryService: BasketHistoryService) {}

  ngOnInit(): void {
    this.loadBasketHistory();
  }

  loadBasketHistory(): void {
    this.loading = true;
    this.error = null;

    this.basketHistoryService.getUserBasketHistory(
      this.pagination.pageNumber,
      this.pagination.pageSize
    ).subscribe({
      next: (response) => {
        this.basketHistory = response.items;
        this.pagination = {
          pageNumber: response.pageNumber,
          pageSize: response.pageSize,
          totalCount: response.totalCount,
          totalPages: response.totalPages
        };
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load basket history';
        this.loading = false;
      }
    });
  }

  onPageChange(event: PageEvent): void {
    this.pagination.pageNumber = event.pageIndex + 1;
    this.pagination.pageSize = event.pageSize;
    this.loadBasketHistory();
  }
}
