import { Environment } from '@abp/ng.core';

const baseUrl = 'http://localhost:4200';

export const environment = {
  production: false,
  application: {
    baseUrl,
    name: 'WareFlow',
    logoUrl: 'assets/images/logo/sleek-wareflow-wordmark.png',
  },
  oAuthConfig: {
    issuer: 'https://localhost:44319/',
    redirectUri: baseUrl,
    clientId: 'WarehouseManagement_App',
    responseType: 'code',
    scope: 'offline_access WarehouseManagement',
    requireHttps: true
  },
  apis: {
    default: {
      url: 'https://localhost:44319',
      rootNamespace: 'WarehouseManagement',
    },
  },
} as Environment;
