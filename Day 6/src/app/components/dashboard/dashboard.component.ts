import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../services/auth.service';

interface User {
  id: number;
  username: string;
  role: string;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="dashboard-container">
      <div class="dashboard-header">
        <h1>Admin Dashboard</h1>
        <button (click)="logout()" class="logout-btn">Logout</button>
      </div>

      <div class="dashboard-content">
        <!-- Add User Form -->
        <div class="add-user-section">
          <h2>Add New User</h2>
          <form [formGroup]="userForm" (ngSubmit)="addUser()">
            <div class="form-group">
              <label for="username">Username/Email:</label>
              <input 
                type="email" 
                id="username" 
                formControlName="username"
                [class.error]="userForm.get('username')?.invalid && userForm.get('username')?.touched"
              >
              <div class="error-message" *ngIf="userForm.get('username')?.invalid && userForm.get('username')?.touched">
                Valid email is required
              </div>
            </div>

            <div class="form-group">
              <label for="password">Password:</label>
              <input 
                type="password" 
                id="password" 
                formControlName="password"
                [class.error]="userForm.get('password')?.invalid && userForm.get('password')?.touched"
              >
              <div class="error-message" *ngIf="userForm.get('password')?.invalid && userForm.get('password')?.touched">
                Password must be at least 6 characters
              </div>
            </div>

            <div class="form-group">
              <label for="role">Role:</label>
              <select id="role" formControlName="role">
                <option value="User">User</option>
                <option value="Admin">Admin</option>
              </select>
            </div>

            <button type="submit" [disabled]="userForm.invalid || loading" class="btn-primary">
              {{ loading ? 'Adding User...' : 'Add User' }}
            </button>

            <div class="success-message" *ngIf="successMessage">
              {{ successMessage }}
            </div>

            <div class="error-message" *ngIf="errorMessage">
              {{ errorMessage }}
            </div>
          </form>
        </div>

        <!-- Users List -->
        <div class="users-section">
          <h2>Current Users</h2>
          <div class="users-list" *ngIf="users.length > 0">
            <div class="user-card" *ngFor="let user of users">
              <div class="user-info">
                <strong>{{ user.username }}</strong>
                <span class="role-badge" [class]="'role-' + user.role.toLowerCase()">
                  {{ user.role }}
                </span>
              </div>
              <button (click)="deleteUser(user.id)" class="delete-btn">Delete</button>
            </div>
          </div>
          <div *ngIf="users.length === 0" class="no-users">
            <p>No users found.</p>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .dashboard-container {
      min-height: 100vh;
      padding: 20px;
    }

    .dashboard-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      background: white;
      padding: 1.5rem 2rem;
      border-radius: 10px;
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
      margin-bottom: 2rem;
    }

    .dashboard-header h1 {
      margin: 0;
      color: #333;
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

    .dashboard-content {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 2rem;
    }

    .add-user-section, .users-section {
      background: white;
      padding: 2rem;
      border-radius: 10px;
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    }

    h2 {
      margin-top: 0;
      color: #333;
      border-bottom: 2px solid #667eea;
      padding-bottom: 0.5rem;
    }

    .form-group {
      margin-bottom: 1.5rem;
    }

    label {
      display: block;
      margin-bottom: 0.5rem;
      font-weight: bold;
      color: #555;
    }

    input, select {
      width: 100%;
      padding: 0.75rem;
      border: 1px solid #ddd;
      border-radius: 5px;
      font-size: 1rem;
      box-sizing: border-box;
    }

    input.error {
      border-color: #e74c3c;
    }

    .btn-primary {
      width: 100%;
      padding: 0.75rem;
      background: #667eea;
      color: white;
      border: none;
      border-radius: 5px;
      font-size: 1rem;
      cursor: pointer;
      transition: background 0.3s;
    }

    .btn-primary:hover:not(:disabled) {
      background: #5a6fd8;
    }

    .btn-primary:disabled {
      background: #ccc;
      cursor: not-allowed;
    }

    .success-message {
      color: #27ae60;
      font-size: 0.875rem;
      margin-top: 1rem;
      padding: 0.5rem;
      background: #d5f4e6;
      border-radius: 5px;
    }

    .error-message {
      color: #e74c3c;
      font-size: 0.875rem;
      margin-top: 0.5rem;
    }

    .users-list {
      max-height: 400px;
      overflow-y: auto;
    }

    .user-card {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 1rem;
      margin-bottom: 0.5rem;
      background: #f8f9fa;
      border-radius: 5px;
      border-left: 4px solid #667eea;
    }

    .user-info {
      display: flex;
      align-items: center;
      gap: 1rem;
    }

    .role-badge {
      padding: 0.25rem 0.5rem;
      border-radius: 15px;
      font-size: 0.75rem;
      font-weight: bold;
      text-transform: uppercase;
    }

    .role-admin {
      background: #e74c3c;
      color: white;
    }

    .role-user {
      background: #3498db;
      color: white;
    }

    .delete-btn {
      padding: 0.25rem 0.5rem;
      background: #e74c3c;
      color: white;
      border: none;
      border-radius: 3px;
      cursor: pointer;
      font-size: 0.875rem;
    }

    .delete-btn:hover {
      background: #c0392b;
    }

    .no-users {
      text-align: center;
      padding: 2rem;
      color: #666;
    }

    @media (max-width: 768px) {
      .dashboard-content {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class DashboardComponent implements OnInit {
  userForm: FormGroup;
  users: User[] = [];
  loading = false;
  successMessage = '';
  errorMessage = '';
  private apiUrl = 'https://localhost:7000/api'; // Update with your API URL

  constructor(
    private formBuilder: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private http: HttpClient
  ) {
    this.userForm = this.formBuilder.group({
      username: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      role: ['User', Validators.required]
    });
  }

  ngOnInit() {
    this.loadUsers();
  }

  addUser() {
    if (this.userForm.invalid) {
      return;
    }

    this.loading = true;
    this.successMessage = '';
    this.errorMessage = '';

    const userData = {
      email: this.userForm.value.username,
      password: this.userForm.value.password,
      confirmPassword: this.userForm.value.password,
      role: this.userForm.value.role
    };

    this.http.post(`${this.apiUrl}/auth/register`, userData).subscribe({
      next: (response) => {
        this.loading = false;
        this.successMessage = 'User added successfully!';
        this.userForm.reset();
        this.userForm.patchValue({ role: 'User' });
        this.loadUsers();
        setTimeout(() => this.successMessage = '', 3000);
      },
      error: (error) => {
        this.loading = false;
        this.errorMessage = error.error?.message || 'Failed to add user. Please try again.';
        setTimeout(() => this.errorMessage = '', 5000);
      }
    });
  }

  loadUsers() {
    // Since we don't have a get users endpoint, we'll maintain a simple list
    // In a real app, you'd call an API endpoint to get all users
    this.users = [
      { id: 1, username: 'admin@test.com', role: 'Admin' },
      { id: 2, username: 'user@test.com', role: 'User' }
    ];
  }

  deleteUser(userId: number) {
    if (confirm('Are you sure you want to delete this user?')) {
      // In a real app, you'd call an API endpoint to delete the user
      this.users = this.users.filter(user => user.id !== userId);
      this.successMessage = 'User deleted successfully!';
      setTimeout(() => this.successMessage = '', 3000);
    }
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
