import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';

@Component({
  selector: 'app-purchases-placeholder',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './purchases-placeholder.component.html',
  styleUrls: ['./purchases-placeholder.component.scss']
})
export class PurchasesPlaceholderComponent implements OnInit {
  private route = inject(ActivatedRoute);

  title = 'Purchases Module';
  icon = 'bi bi-bag-check';
  description = 'This module is scheduled for future implementation in the Purchases workflow.';

  ngOnInit(): void {
    const data = this.route.snapshot.data;
    if (data['title']) this.title = data['title'];
    if (data['icon']) this.icon = data['icon'];
    if (data['description']) this.description = data['description'];
  }
}

