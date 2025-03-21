import {BasketHistoryItem} from './basket-history-item.model';

export interface BasketHistory {
  id: number;
  createdAt: string;
  totalAmount: number;
  totalDiscount: number;
  finalAmount: number;
  items: BasketHistoryItem[];
}
