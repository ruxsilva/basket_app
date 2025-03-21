import { DiscountedItemModel } from './discounted-item.model';
import {ReceiptItemModel} from './receipt.item.model';

export interface ReceiptModel {
  items: ReceiptItemModel[];
  discounts: DiscountedItemModel[];
  totalBeforeDiscount: number;
  totalDiscount: number;
  finalTotal: number;
}
