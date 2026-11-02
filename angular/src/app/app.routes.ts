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
    loadChildren: () => import('./cycle-counts/cycle-counts.routes').then(m => m.cycleCountsRoutes),
    canActivate: [AuthGuard],
  },
  {
    path: 'suppliers',
    loadChildren: () => import('./suppliers/suppliers.routes').then(m => m.suppliersRoutes),
    canActivate: [AuthGuard],
  },
  {
    path: 'customers',
    loadChildren: () => import('./customers/customers.routes').then(m => m.customersRoutes),
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
    loadChildren: () => import('./returns/returns.routes').then(m => m.returnsRoutes),
    canActivate: [AuthGuard],
  },
  {
    path: 'reports',
    loadChildren: () => import('./reports/reports.routes').then(m => m.reportsRoutes),
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
