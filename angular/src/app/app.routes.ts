import { Routes } from '@angular/router';
import { AuthGuard } from '@abp/ng.core';

export const appRoutes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    loadChildren: () => import('./home/home.routes').then(m => m.homeRoutes),
    canActivate: [AuthGuard],
  },
  {
    path: 'warehouses',
    loadChildren: () => import('./warehouses/warehouses.routes').then(m => m.warehousesRoutes),
    canActivate: [AuthGuard],
  },
  {
    path: 'faculty',
    loadChildren: () => import('./faculty/faculty.routes').then(m => m.facultyRoutes),
    canActivate: [AuthGuard],
  },
  {
    path: 'categories',
    loadChildren: () => import('./categories/categories.routes').then(m => m.categoriesRoutes),
    canActivate: [AuthGuard],
  },
  {
    path: 'products',
    loadChildren: () => import('./products/products.routes').then(m => m.productsRoutes),
    canActivate: [AuthGuard],
  },
  {
    path: 'inventory',
    loadChildren: () => import('./inventory/inventory.routes').then(m => m.inventoryRoutes),
    canActivate: [AuthGuard],
  },
  {
    path: 'transfers',
    loadChildren: () => import('./transfers/transfers.routes').then(m => m.transfersRoutes),
    canActivate: [AuthGuard],
  },
  {
    path: 'cycle-counts',
    loadComponent: () =>
      import('./shared/components/module-placeholder.component').then(m => m.ModulePlaceholderComponent),
    data: {
      title: 'Cycle Counting & Stock Audits',
      badge: 'Inventory Reconciliation',
      icon: 'bi bi-clipboard-check',
      subtitle: 'Periodic physical inventory verification, variance reconciliation & audit approvals.',
      features: [
        'ABC classification-based periodic count scheduling (Daily / Weekly)',
        'Blind counting mode to ensure 100% accurate physical verification',
        'Automatic variance detection between system stock vs actual count',
        'Managerial variance review, sign-off & financial write-off approvals',
        'Discrepancy reason code tracking & shrinkage analytics',
      ],
    },
    canActivate: [AuthGuard],
  },
  {
    path: 'suppliers',
    loadChildren: () => import('./suppliers/suppliers.routes').then(m => m.suppliersRoutes),
    canActivate: [AuthGuard],
  },
  {
    path: 'customers',
    loadComponent: () =>
      import('./shared/components/module-placeholder.component').then(m => m.ModulePlaceholderComponent),
    data: {
      title: 'Customers & B2B Clients',
      badge: 'Client Directory',
      icon: 'bi bi-people',
      subtitle: 'Manage client accounts, delivery locations, GSTIN tax credentials & credit terms.',
      features: [
        'B2B Corporate & B2C customer master profiling',
        'Multiple shipping destinations, dock hours & delivery instructions',
        'GSTIN, PAN & tax compliance verification for invoicing',
        'Payment terms (Net 30, Advance, COD) & credit limit enforcement',
        'Seamless integration with Outbound Orders customer selector',
      ],
    },
    canActivate: [AuthGuard],
  },
  {
    path: 'inbound',
    loadChildren: () => import('./inbound/inbound.routes').then(m => m.inboundRoutes),
    canActivate: [AuthGuard],
  },
  {
    path: 'outbound',
    loadChildren: () => import('./outbound/outbound.routes').then(m => m.outboundRoutes),
    canActivate: [AuthGuard],
  },
  {
    path: 'returns',
    loadComponent: () =>
      import('./shared/components/module-placeholder.component').then(m => m.ModulePlaceholderComponent),
    data: {
      title: 'Returns & Reverse Logistics (RMA)',
      badge: 'RTO / RMA Hub',
      icon: 'bi bi-arrow-return-left',
      subtitle: 'Receive customer returns, inspect quality & route to restock, refurbish or scrap.',
      features: [
        'RMA (Return Merchandise Authorization) order generation & tracking',
        'Return dock receiving with carrier AWB & customer invoice matching',
        'Quality grading inspection (Grade A / Like New, B / Open Box, Damaged)',
        'Automated bin putaway routing: Restock to inventory or Scrap bin',
        'Return reason analytics (Defective, Wrong Item, Buyer Remorse)',
      ],
    },
    canActivate: [AuthGuard],
  },
  {
    path: 'reports',
    loadComponent: () =>
      import('./shared/components/module-placeholder.component').then(m => m.ModulePlaceholderComponent),
    data: {
      title: 'Reports & Warehouse Analytics',
      badge: 'Business Intelligence',
      icon: 'bi bi-bar-chart-line',
      subtitle: 'Comprehensive inventory valuation, aging reports, turnover metrics & operational KPIs.',
      features: [
        'Stock Aging Report (0-30, 31-60, 61-90, 90+ days inventory age)',
        'FSN Analysis (Fast-moving, Slow-moving & Non-moving dead stock)',
        'Inventory valuation summary by category, warehouse & FIFO cost',
        'Dock-to-Stock inbound receiving turnaround efficiency reports',
        'Outbound fulfillment SLA & pick-pack worker productivity metrics',
      ],
    },
    canActivate: [AuthGuard],
  },
  {
    path: 'account',
    loadChildren: () => import('@abp/ng.account').then(m => m.createRoutes()),
  },
  {
    path: 'identity',
    loadChildren: () => import('@abp/ng.identity').then(m => m.createRoutes()),
    canActivate: [AuthGuard],
  },
  {
    path: 'tenant-management',
    loadChildren: () =>
      import('@abp/ng.tenant-management').then(m => m.createRoutes()),
    canActivate: [AuthGuard],
  },
  {
    path: 'setting-management',
    loadChildren: () =>
      import('@abp/ng.setting-management').then(m => m.createRoutes()),
    canActivate: [AuthGuard],
  },
];
