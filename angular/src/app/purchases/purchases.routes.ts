import { Routes } from '@angular/router';
import { PurchaseReceivesComponent } from './purchase-receives/purchase-receives.component';
import { BillsComponent } from './bills/bills.component';
import { PaymentsModeComponent } from './payments-mode/payments-mode.component';
import { VendorCreditsComponent } from './vendor-credits/vendor-credits.component';

export const purchasesRoutes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'purchase-receives',
  },
  {
    path: 'purchase-receives',
    component: PurchaseReceivesComponent,
  },
  {
    path: 'bills',
    component: BillsComponent,
  },
  {
    path: 'payments-mode',
    component: PaymentsModeComponent,
  },
  {
    path: 'vendor-credits',
    component: VendorCreditsComponent,
  }
];

