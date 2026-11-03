import { Component, OnInit, OnDestroy, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { InternetConnectionStatusComponent, LoaderBarComponent } from '@abp/ng.theme.shared';
import { DynamicLayoutComponent } from '@abp/ng.core';

@Component({
  selector: 'app-root',
  template: `
    <abp-loader-bar />
    <abp-dynamic-layout />
    <abp-internet-status />
  `,
  imports: [LoaderBarComponent, DynamicLayoutComponent, InternetConnectionStatusComponent],
})
export class AppComponent implements OnInit, OnDestroy {
  private modalObserver?: MutationObserver;

  constructor(@Inject(PLATFORM_ID) private platformId: Object) {}

  ngOnInit(): void {
    if (isPlatformBrowser(this.platformId)) {
      this.initModalScrollLockObserver();
    }
  }

  ngOnDestroy(): void {
    if (this.modalObserver) {
      this.modalObserver.disconnect();
    }
    if (typeof document !== 'undefined') {
      document.body?.classList.remove('wms-modal-open');
      document.documentElement?.classList.remove('wms-modal-open');
    }
  }

  private initModalScrollLockObserver(): void {
    const selector = [
      '.modal.show',
      '.modal.d-block',
      '.modal-backdrop',
      '.drawer-backdrop',
      '.category-drawer-modal',
      '.product-drawer-modal',
      '.wh-drawer-modal',
      '.faculty-drawer-modal',
      '.inbound-drawer-modal',
      '.outbound-drawer-modal',
      '.customer-drawer-panel.open',
      '.returns-drawer-panel.open',
      '.supplier-drawer-panel.open'
    ].join(',');

    let rafId: number | null = null;
    const checkModals = () => {
      const isAnyModalOpen = !!document.querySelector(selector);
      if (isAnyModalOpen) {
        if (!document.body.classList.contains('wms-modal-open')) {
          document.body.classList.add('wms-modal-open');
          document.documentElement.classList.add('wms-modal-open');
        }
      } else {
        if (document.body.classList.contains('wms-modal-open')) {
          document.body.classList.remove('wms-modal-open');
          document.documentElement.classList.remove('wms-modal-open');
        }
      }
    };

    this.modalObserver = new MutationObserver(() => {
      if (rafId !== null) cancelAnimationFrame(rafId);
      rafId = requestAnimationFrame(checkModals);
    });

    this.modalObserver.observe(document.body, {
      childList: true,
      subtree: true,
      attributes: true,
      attributeFilter: ['class', 'style']
    });

    // Initial check
    checkModals();
  }
}
