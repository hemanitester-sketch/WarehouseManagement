import { Directive, EventEmitter, HostListener, Output, Input, OnDestroy } from '@angular/core';

@Directive({
  selector: '[appBarcodeScanner]',
  standalone: true
})
export class BarcodeScannerDirective implements OnDestroy {
  @Input() scanThresholdMs = 50;
  @Input() minBarcodeLength = 3;
  @Output() barcodeScanned = new EventEmitter<string>();

  private buffer: string = '';
  private lastKeyTime: number = 0;
  private timer: any = null;

  @HostListener('window:keydown', ['$event'])
  onKeyDown(event: KeyboardEvent): void {
    // Ignore control key combinations
    if (event.ctrlKey || event.altKey || event.metaKey) {
      return;
    }

    const currentTime = Date.now();
    const isEnter = event.key === 'Enter';

    if (isEnter) {
      if (this.buffer.length >= this.minBarcodeLength) {
        event.preventDefault();
        const scanned = this.buffer.trim();
        this.buffer = '';
        this.barcodeScanned.emit(scanned);
      }
      this.buffer = '';
      return;
    }

    if (event.key.length === 1) {
      const timeDiff = currentTime - this.lastKeyTime;
      if (timeDiff > 250 && this.buffer.length > 0) {
        this.buffer = '';
      }

      this.buffer += event.key;
      this.lastKeyTime = currentTime;

      clearTimeout(this.timer);
      this.timer = setTimeout(() => {
        this.buffer = '';
      }, 500);
    }
  }

  ngOnDestroy(): void {
    if (this.timer) {
      clearTimeout(this.timer);
    }
  }
}

