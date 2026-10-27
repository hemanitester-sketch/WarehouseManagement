import { Environment } from '@abp/ng.core';

const baseUrl = typeof window !== 'undefined' ? window.location.origin : 'http://localhost:4200';

export const environment = {
  production: true,
  application: {
    baseUrl,
    name: 'Warehouse Management',
    logoUrl: '',
  },
  oAuthConfig: {
    issuer: 'https://warehousemanagement-tewt.onrender.com/',
    redirectUri: baseUrl,
    clientId: 'WarehouseManagement_App',
    responseType: 'code',
    scope: 'offline_access WarehouseManagement',
    requireHttps: true
  },
  apis: {
    default: {
      url: 'https://warehousemanagement-tewt.onrender.com',
      rootNamespace: 'WarehouseManagement',
    },
  },
} as Environment;
