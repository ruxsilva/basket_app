import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReceiptModel } from '../../models/receipt.model';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
@Component({
  selector: 'app-receipt-display',
  templateUrl: './receipt-display.component.html',
  styleUrls: ['./receipt-display.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatDividerModule,
  ]
})
export class ReceiptDisplayComponent {
  @Input() receipt: ReceiptModel | null = null;
}
