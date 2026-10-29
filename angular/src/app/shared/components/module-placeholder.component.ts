import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-module-placeholder',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container-fluid placeholder-page-container p-4">
      <!-- Standard Page Header -->
      <div class="d-flex flex-wrap justify-content-between align-items-center mb-4 gap-2">
        <div>
          <div class="d-flex align-items-center gap-2">
            <h2 class="h4 font-weight-bold text-dark mb-0">{{ title }}</h2>
            <span class="wms-badge wms-badge-primary">{{ badge }}</span>
          </div>
          <p class="text-muted small mb-0 mt-1">
            {{ subtitle }}
          </p>
        </div>
        <div>
          <span class="badge bg-primary-subtle text-primary border border-primary-subtle px-3 py-2 fw-semibold">
            <i class="bi bi-clock-history me-1"></i> Planned WMS Module
          </span>
        </div>
      </div>

      <!-- Feature Blueprint Card -->
      <div class="card border-0 shadow-sm p-4" style="border-radius: 12px; background: #ffffff;">
        <div class="row align-items-center g-4">
          <div class="col-md-7">
            <div class="d-flex align-items-center gap-3 mb-3">
              <div class="icon-box-preview">
                <i [class]="icon"></i>
              </div>
              <div>
                <h5 class="mb-0 fw-bold text-dark">{{ title }}</h5>
                <small class="text-muted">Enterprise WMS Architecture Specification</small>
              </div>
            </div>

            <p class="text-muted" style="font-size: 0.85rem; line-height: 1.6;">
              Yeh module sidebar navigation me successfully place ho gaya hai. Iske andar niche diye gaye core industry features develop kiye ja sakte hain:
            </p>

            <div class="features-list">
              <div *ngFor="let feat of features" class="d-flex align-items-start gap-2 mb-2">
                <i class="bi bi-check2-circle text-primary mt-1" style="font-size: 0.95rem;"></i>
                <span class="text-secondary" style="font-size: 0.825rem; font-weight: 500;">{{ feat }}</span>
              </div>
            </div>
          </div>

          <div class="col-md-5 text-center">
            <div class="placeholder-art-box p-4 rounded-3 border" style="background: #f8fafc; border-color: #e2e8f0 !important;">
              <i [class]="icon + ' text-primary opacity-75'" style="font-size: 3.5rem;"></i>
              <h6 class="mt-3 mb-1 fw-bold text-dark">Ready for Full-Stack Implementation</h6>
              <p class="text-muted small mb-0">
                Backend Domain Entities, EF Core Migrations, REST API & Angular UI ready to be configured.
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .placeholder-page-container {
      animation: fadeIn 0.2s ease-in;
    }
    @keyframes fadeIn {
      from { opacity: 0; transform: translateY(4px); }
      to { opacity: 1; transform: translateY(0); }
    }
    .icon-box-preview {
      width: 48px;
      height: 48px;
      border-radius: 12px;
      background: #eff6ff;
      color: #2563eb;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 1.5rem;
    }
  `]
})
export class ModulePlaceholderComponent {
  private route = inject(ActivatedRoute);

  get title(): string {
    return this.route.snapshot.data['title'] || 'WMS Module';
  }

  get badge(): string {
    return this.route.snapshot.data['badge'] || 'Enterprise Module';
  }

  get icon(): string {
    return this.route.snapshot.data['icon'] || 'bi bi-grid';
  }

  get subtitle(): string {
    return this.route.snapshot.data['subtitle'] || 'Next generation enterprise warehouse management module.';
  }

  get features(): string[] {
    return this.route.snapshot.data['features'] || [];
  }
}

