import { Environment } from '@abp/ng.core';

const baseUrl = 'http://localhost:4200';

export const environment = {
  production: false,
  application: {
    baseUrl,
    name: 'WareFlow',
    logoUrl: '',
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
