import {ItemModel} from './item.model';

export interface BasketItemModel {
  item: ItemModel;
  lineTotal: number;
  quantity: number;
}
