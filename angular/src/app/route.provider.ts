import { RoutesService, eLayoutType } from '@abp/ng.core';
import { APP_INITIALIZER } from '@angular/core';

export const APP_ROUTE_PROVIDER = [
  { provide: APP_INITIALIZER, useFactory: configureRoutes, deps: [RoutesService], multi: true },
];

function configureRoutes(routesService: RoutesService) {
  return () => {
    routesService.add([
      {
        path: '/',
        name: 'Dashboard',
        iconClass: 'bi bi-speedometer2',
        order: 1,
        layout: eLayoutType.application,
      },
      {
        path: '/warehouses',
        name: 'Warehouses & Zones',
        iconClass: 'bi bi-building',
        order: 2,
        layout: eLayoutType.application,
      },
      {
        path: '/faculty',
        name: 'Faculty & Managers',
        iconClass: 'bi bi-person-badge',
        order: 3,
        layout: eLayoutType.application,
      },
      {
        path: undefined,
        name: 'Products & Catalog',
        iconClass: 'bi bi-boxes',
        order: 4,
        layout: eLayoutType.application,
      },
      {
        path: '/categories',
        name: 'Categories',
        parentName: 'Products & Catalog',
        iconClass: 'bi bi-tags',
        order: 1,
        layout: eLayoutType.application,
      },
      {
        path: '/products',
        name: 'Products & SKUs',
        parentName: 'Products & Catalog',
        iconClass: 'bi bi-box-seam',
        order: 2,
        layout: eLayoutType.application,
      },
      {
        path: undefined,
        name: 'Inventory Operations',
        iconClass: 'bi bi-layers',
        order: 5,
        layout: eLayoutType.application,
      },
      {
        path: '/inventory',
        name: 'Stock Inventory',
        parentName: 'Inventory Operations',
        iconClass: 'bi bi-stack',
        order: 1,
        layout: eLayoutType.application,
      },
      {
        path: '/transfers',
        name: 'Stock Transfers & Movement',
        parentName: 'Inventory Operations',
        iconClass: 'bi bi-arrow-left-right',
        order: 2,
        layout: eLayoutType.application,
      },
      {
        path: '/cycle-counts',
        name: 'Cycle Counts & Audits',
        parentName: 'Inventory Operations',
        iconClass: 'bi bi-clipboard-check',
        order: 3,
        layout: eLayoutType.application,
      },
      {
        path: '/suppliers',
        name: 'Suppliers & Vendors',
        iconClass: 'bi bi-shop',
        order: 6,
        layout: eLayoutType.application,
      },
      {
        path: '/customers',
        name: 'Customers & B2B Clients',
        iconClass: 'bi bi-people',
        order: 7,
        layout: eLayoutType.application,
      },
      {
        path: '/inbound',
        name: 'Inbound Orders',
        iconClass: 'bi bi-truck',
        order: 8,
        layout: eLayoutType.application,
      },
      {
        path: '/outbound',
        name: 'Outbound Orders',
        iconClass: 'bi bi-send',
        order: 9,
        layout: eLayoutType.application,
      },
      {
        path: '/returns',
        name: 'Returns & RMA',
        iconClass: 'bi bi-arrow-return-left',
        order: 10,
        layout: eLayoutType.application,
      },
      {
        path: '/reports',
        name: 'Reports & Analytics',
        iconClass: 'bi bi-bar-chart-line',
        order: 11,
        layout: eLayoutType.application,
      },
    ]);
  };
}
