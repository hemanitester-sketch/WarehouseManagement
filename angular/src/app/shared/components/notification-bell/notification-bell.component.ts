import { Component, ElementRef, HostListener, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface WarehouseNotification {
  id: string;
  title: string;
  message: string;
  time: string;
  type: 'warning' | 'info' | 'success';
  icon: string;
  read: boolean;
}

@Component({
  selector: 'app-notification-bell',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="notification-wrapper">
      <button
        type="button"
        class="notification-btn"
        (click)="toggleDropdown($event)"
        [attr.aria-expanded]="isOpen()"
        title="Notifications"
      >
        <i class="bi bi-bell"></i>
        @if (unreadCount() > 0) {
          <span class="notification-badge">{{ unreadCount() }}</span>
        }
      </button>

      @if (isOpen()) {
        <div class="notification-dropdown" (click)="$event.stopPropagation()">
          <div class="notification-header">
            <div class="header-title">
              <span class="title-text">Notifications</span>
              @if (unreadCount() > 0) {
                <span class="badge-count">{{ unreadCount() }} New</span>
              }
            </div>
            @if (unreadCount() > 0) {
              <button type="button" class="mark-read-btn" (click)="markAllAsRead()">
                Mark all read
              </button>
            }
          </div>

          <div class="notification-list">
            @for (item of notifications(); track item.id) {
              <div
                class="notification-item"
                [class.unread]="!item.read"
                (click)="markAsRead(item)"
              >
                <div class="item-icon" [ngClass]="item.type">
                  <i [class]="item.icon"></i>
                </div>
                <div class="item-content">
                  <div class="item-title">{{ item.title }}</div>
                  <div class="item-message">{{ item.message }}</div>
                  <div class="item-time">
                    <i class="bi bi-clock me-1"></i>{{ item.time }}
                  </div>
                </div>
                @if (!item.read) {
                  <span class="unread-dot"></span>
                }
              </div>
            } @empty {
              <div class="empty-state">
                <i class="bi bi-bell-slash"></i>
                <p>No new notifications</p>
              </div>
            }
          </div>

          <div class="notification-footer">
            <span class="footer-text">Warehouse Activity Feed</span>
          </div>
        </div>
      }
    </div>
  `,
  styles: [`
    :host {
      display: inline-flex;
      align-items: center;
      margin-right: 0.75rem;
    }

    .notification-wrapper {
      position: relative;
      display: inline-flex;
      align-items: center;
    }

    .notification-btn {
      position: relative;
      background: #f8fafc;
      border: 1px solid #e2e8f0;
      border-radius: 50%;
      width: 40px;
      height: 40px;
      display: flex;
      align-items: center;
      justify-content: center;
      color: #475569;
      font-size: 1.15rem;
      cursor: pointer;
      transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
      padding: 0;
      outline: none;

      &:hover {
        background: #f1f5f9;
        color: #2563eb;
        border-color: #cbd5e1;
        box-shadow: 0 2px 8px rgba(37, 99, 235, 0.12);
        transform: translateY(-1px);
      }

      &:active {
        transform: translateY(0);
      }
    }

    .notification-badge {
      position: absolute;
      top: -2px;
      right: -2px;
      background: #ef4444;
      color: #ffffff;
      font-size: 0.68rem;
      font-weight: 700;
      min-width: 18px;
      height: 18px;
      border-radius: 9px;
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 0 4px;
      border: 2px solid #ffffff;
      box-shadow: 0 2px 4px rgba(239, 68, 68, 0.3);
      animation: pulse 2s infinite;
    }

    @keyframes pulse {
      0% {
        box-shadow: 0 0 0 0 rgba(239, 68, 68, 0.5);
      }
      70% {
        box-shadow: 0 0 0 6px rgba(239, 68, 68, 0);
      }
      100% {
        box-shadow: 0 0 0 0 rgba(239, 68, 68, 0);
      }
    }

    .notification-dropdown {
      position: absolute;
      top: calc(100% + 10px);
      right: 0;
      width: 340px;
      max-width: 90vw;
      background: #ffffff;
      border: 1px solid #edf2f7;
      border-radius: 14px;
      box-shadow: 0 16px 36px -8px rgba(15, 23, 42, 0.16), 0 0 1px 1px rgba(226, 232, 240, 0.8);
      z-index: 1050;
      overflow: hidden;
      animation: dropdownIn 0.2s cubic-bezier(0.16, 1, 0.3, 1);
    }

    @keyframes dropdownIn {
      from {
        opacity: 0;
        transform: translateY(-8px) scale(0.98);
      }
      to {
        opacity: 1;
        transform: translateY(0) scale(1);
      }
    }

    .notification-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 0.85rem 1rem;
      background: #f8fafc;
      border-bottom: 1px solid #edf2f7;

      .header-title {
        display: flex;
        align-items: center;
        gap: 0.5rem;

        .title-text {
          font-weight: 700;
          font-size: 0.92rem;
          color: #0f172a;
        }

        .badge-count {
          background: #eff6ff;
          color: #2563eb;
          font-size: 0.72rem;
          font-weight: 600;
          padding: 2px 8px;
          border-radius: 12px;
          border: 1px solid #dbeafe;
        }
      }

      .mark-read-btn {
        background: none;
        border: none;
        color: #2563eb;
        font-size: 0.75rem;
        font-weight: 600;
        cursor: pointer;
        padding: 0;
        &:hover {
          text-decoration: underline;
        }
      }
    }

    .notification-list {
      max-height: 320px;
      overflow-y: auto;
    }

    .notification-item {
      display: flex;
      align-items: flex-start;
      gap: 0.75rem;
      padding: 0.85rem 1rem;
      border-bottom: 1px solid #f1f5f9;
      cursor: pointer;
      transition: background 0.15s ease;
      position: relative;

      &:hover {
        background: #f8fafc;
      }

      &.unread {
        background: #f8faff;
      }

      &:last-child {
        border-bottom: none;
      }

      .item-icon {
        width: 34px;
        height: 34px;
        border-radius: 8px;
        display: flex;
        align-items: center;
        justify-content: center;
        flex-shrink: 0;
        font-size: 1rem;

        &.warning {
          background: #fffbeb;
          color: #d97706;
          border: 1px solid #fef3c7;
        }
        &.info {
          background: #eff6ff;
          color: #2563eb;
          border: 1px solid #dbeafe;
        }
        &.success {
          background: #f0fdf4;
          color: #16a34a;
          border: 1px solid #dcfce7;
        }
      }

      .item-content {
        flex: 1;
        min-width: 0;

        .item-title {
          font-size: 0.82rem;
          font-weight: 600;
          color: #1e293b;
          margin-bottom: 0.15rem;
        }

        .item-message {
          font-size: 0.76rem;
          color: #64748b;
          line-height: 1.35;
          margin-bottom: 0.25rem;
        }

        .item-time {
          font-size: 0.68rem;
          color: #94a3b8;
          display: flex;
          align-items: center;
        }
      }

      .unread-dot {
        width: 7px;
        height: 7px;
        background: #2563eb;
        border-radius: 50%;
        margin-top: 6px;
        flex-shrink: 0;
      }
    }

    .empty-state {
      padding: 2rem 1rem;
      text-align: center;
      color: #94a3b8;

      i {
        font-size: 2rem;
        display: block;
        margin-bottom: 0.5rem;
      }

      p {
        margin: 0;
        font-size: 0.85rem;
      }
    }

    .notification-footer {
      padding: 0.6rem 1rem;
      background: #f8fafc;
      border-top: 1px solid #edf2f7;
      text-align: center;

      .footer-text {
        font-size: 0.72rem;
        font-weight: 500;
        color: #94a3b8;
        letter-spacing: 0.02em;
      }
    }
  `]
})
export class NotificationBellComponent {
  isOpen = signal<boolean>(false);

  notifications = signal<WarehouseNotification[]>([
    {
      id: '1',
      title: 'Low Stock Alert',
      message: 'Product "Barcode Scanner Pro" has fallen below safety limit (6 units left).',
      time: '12m ago',
      type: 'warning',
      icon: 'bi bi-exclamation-triangle-fill',
      read: false,
    },
    {
      id: '2',
      title: 'Inbound Shipment Received',
      message: 'Order #INB-2026-44 arrived at Main Distribution Center (Bay 3).',
      time: '45m ago',
      type: 'info',
      icon: 'bi bi-box-seam-fill',
      read: false,
    },
    {
      id: '3',
      title: 'Outbound Dispatch Completed',
      message: 'Dispatched 48 cartons for Logistics Order #OUT-9018.',
      time: '2h ago',
      type: 'success',
      icon: 'bi bi-check-circle-fill',
      read: false,
    },
  ]);

  unreadCount = signal<number>(3);

  constructor(private elementRef: ElementRef) {}

  toggleDropdown(event: Event): void {
    event.stopPropagation();
    this.isOpen.update((v) => !v);
  }

  markAsRead(item: WarehouseNotification): void {
    if (!item.read) {
      item.read = true;
      this.unreadCount.update((count) => Math.max(0, count - 1));
    }
  }

  markAllAsRead(): void {
    this.notifications.update((list) =>
      list.map((item) => ({ ...item, read: true }))
    );
    this.unreadCount.set(0);
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (!this.elementRef.nativeElement.contains(event.target)) {
      this.isOpen.set(false);
    }
  }
}
