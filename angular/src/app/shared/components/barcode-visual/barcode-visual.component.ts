import { Component, Input, OnChanges } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-barcode-visual',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="barcode-visual-wrapper d-inline-flex flex-column align-items-center">
      <svg
        [attr.width]="svgWidth"
        [attr.height]="height"
        class="barcode-svg"
        xmlns="http://www.w3.org/2000/svg"
      >
        <rect width="100%" height="100%" fill="#ffffff" />
        <g fill="#000000">
          <rect
            *ngFor="let bar of bars"
            [attr.x]="bar.x"
            [attr.y]="0"
            [attr.width]="bar.width"
            [attr.height]="height - (showText ? 16 : 0)"
          />
        </g>
      </svg>
      <span *ngIf="showText" class="barcode-human-readable font-monospace">
        {{ value }}
      </span>
    </div>
  `,
  styles: [`
    .barcode-visual-wrapper {
      background: #ffffff;
      padding: 6px 10px;
      border: 1px solid #e2e8f0;
      border-radius: 6px;
      user-select: none;
    }
    .barcode-svg {
      display: block;
    }
    .barcode-human-readable {
      font-size: 0.72rem;
      font-weight: 700;
      letter-spacing: 0.15em;
      color: #0f172a;
      margin-top: 3px;
    }
  `]
})
export class BarcodeVisualComponent implements OnChanges {
  @Input() value = '';
  @Input() height = 50;
  @Input() barWidth = 2;
  @Input() showText = true;

  bars: { x: number; width: number }[] = [];
  svgWidth = 160;

  ngOnChanges(): void {
    this.generateBars();
  }

  private generateBars(): void {
    if (!this.value) {
      this.bars = [];
      this.svgWidth = 100;
      return;
    }

    // Deterministic visual pseudo-Code128 pattern based on characters
    const text = this.value.toUpperCase();
    const barList: { x: number; width: number }[] = [];
    let currentX = 10;

    // Start guard pattern (thin-thick-thin)
    barList.push({ x: currentX, width: this.barWidth });
    currentX += this.barWidth * 2;
    barList.push({ x: currentX, width: this.barWidth * 2 });
    currentX += this.barWidth * 3;
    barList.push({ x: currentX, width: this.barWidth });
    currentX += this.barWidth * 2;

    for (let i = 0; i < text.length; i++) {
      const code = text.charCodeAt(i);
      const w1 = (code % 3) + 1;
      const space1 = ((code >> 1) % 2) + 1;
      const w2 = ((code >> 2) % 3) + 1;
      const space2 = ((code >> 3) % 2) + 1;

      barList.push({ x: currentX, width: w1 * this.barWidth });
      currentX += (w1 + space1) * this.barWidth;

      barList.push({ x: currentX, width: w2 * this.barWidth });
      currentX += (w2 + space2) * this.barWidth;
    }

    // End guard pattern
    barList.push({ x: currentX, width: this.barWidth * 2 });
    currentX += this.barWidth * 3;
    barList.push({ x: currentX, width: this.barWidth });
    currentX += this.barWidth * 2;
    barList.push({ x: currentX, width: this.barWidth * 2 });
    currentX += this.barWidth * 3 + 10;

    this.bars = barList;
    this.svgWidth = Math.max(140, currentX);
  }
}

