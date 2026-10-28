import { inject, provideAppInitializer } from '@angular/core';
import { NavItemsService } from '@abp/ng.theme.shared';
import { NotificationBellComponent } from './notification-bell.component';

export const NOTIFICATION_BELL_PROVIDER = provideAppInitializer(() => {
  const navItems = inject(NavItemsService);
  navItems.addItems([
    {
      id: 'WarehouseNotificationBell',
      order: 1,
      component: NotificationBellComponent,
    },
  ]);
});
