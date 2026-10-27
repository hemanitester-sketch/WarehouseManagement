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
        iconClass: 'fas fa-chart-line',
        order: 1,
        layout: eLayoutType.application,
      },
      {
        path: '/warehouses',
        name: 'Warehouses & Zones',
        iconClass: 'fas fa-warehouse',
        order: 2,
        layout: eLayoutType.application,
      },
      {
        path: '/products',
        name: 'Products & SKUs',
        iconClass: 'fas fa-boxes',
        order: 3,
        layout: eLayoutType.application,
      },
      {
        path: '/inventory',
        name: 'Stock Inventory',
        iconClass: 'fas fa-layer-group',
        order: 4,
        layout: eLayoutType.application,
      },
      {
        path: '/inbound',
        name: 'Inbound Orders',
        iconClass: 'fas fa-truck-loading',
        order: 5,
        layout: eLayoutType.application,
      },
      {
        path: '/outbound',
        name: 'Outbound Orders',
        iconClass: 'fas fa-shipping-fast',
        order: 6,
        layout: eLayoutType.application,
      },
    ]);
  };
}
