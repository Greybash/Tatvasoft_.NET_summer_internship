import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="home-container">
      <div class="home-card">
        <div class="header">
          <h1>Hi {{ userName }}!</h1>
          <button (click)="logout()" class="logout-btn">Logout</button>
        </div>
        <div class="welcome-message">
          <p>Welcome to the Movies App!</p>
          <p>You are logged in as a <strong>{{ userRole }}</strong>.</p>
        </div>
        <div class="user-info">
          <h3>Your Account Details:</h3>
          <p><strong>Email:</strong> {{ userEmail }}</p>
          <p><strong>Role:</strong> {{ userRole }}</p>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .home-container {
      min-height: 100vh;
      padding: 20px;
      display: flex;
      justify-content: center;
      align-items: center;
    }

    .home-card {
      background: white;
      padding: 3rem;
      border-radius: 15px;
      box-shadow: 0 8px 16px rgba(0, 0, 0, 0.1);
      width: 100%;
      max-width: 600px;
      text-align: center;
    }

    .header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 2rem;
    }

    h1 {
      color: #333;
      margin: 0;
      font-size: 2.5rem;
    }

    .logout-btn {
      padding: 0.5rem 1rem;
      background: #e74c3c;
      color: white;
      border: none;
      border-radius: 5px;
      cursor: pointer;
      transition: background 0.3s;
    }

    .logout-btn:hover {
      background: #c0392b;
    }

    .welcome-message {
      margin: 2rem 0;
      padding: 2rem;
      background: #f8f9fa;
      border-radius: 10px;
    }

    .welcome-message p {
      font-size: 1.2rem;
      margin: 0.5rem 0;
      color: #666;
    }

    .user-info {
      margin-top: 2rem;
      padding: 1.5rem;
      background: #e8f4fd;
      border-radius: 10px;
    }

    .user-info h3 {
      margin-top: 0;
      color: #2c3e50;
    }

    .user-info p {
      margin: 0.5rem 0;
      color: #34495e;
    }
  `]
})
export class HomeComponent {
  userName: string = '';
  userEmail: string = '';
  userRole: string = '';

  constructor(
    private authService: AuthService,
    private router: Router
  ) {
    const currentUser = this.authService.currentUserValue;
    if (currentUser) {
      this.userName = currentUser.name;
      this.userEmail = currentUser.email;
      this.userRole = currentUser.role;
    }
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
