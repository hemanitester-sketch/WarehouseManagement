import { Environment } from '@abp/ng.core';

const baseUrl = typeof window !== 'undefined' ? window.location.origin : 'http://localhost:4200';

export const environment = {
  production: true,
  application: {
    baseUrl,
    name: 'WareFlow',
    logoUrl: 'assets/images/logo/sleek-wareflow-wordmark.png',
  },
  oAuthConfig: {
    issuer: 'https://warehouseflow-api.runasp.net/',
    redirectUri: baseUrl,
    clientId: 'WarehouseManagement_App',
    responseType: 'code',
    scope: 'offline_access WarehouseManagement',
    requireHttps: true
  },
  apis: {
    default: {
      url: 'https://warehouseflow-api.runasp.net',
      rootNamespace: 'WarehouseManagement',
    },
  },
} as Environment;
