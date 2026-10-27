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
