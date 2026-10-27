import { AuthService } from '@abp/ng.core';
import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
  imports: [CommonModule, RouterLink],
})
export class HomeComponent implements OnInit {
  private authService = inject(AuthService);

  get hasLoggedIn(): boolean {
    return this.authService.isAuthenticated;
  }

  ngOnInit() {
    if (!this.hasLoggedIn) {
      this.login();
    }
  }

  login() {
    this.authService.navigateToLogin();
  }
}
