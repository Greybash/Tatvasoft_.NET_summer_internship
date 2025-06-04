import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { AuthService } from '../../services/auth.service';

interface User {
  id: number;
  username: string;
  name: string;
  phoneNumber: string;
  roleId: number;
  role?: string;
  isActive?: boolean;
}

interface Role {
  id: number;
  name: string;
}

@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule], 
  template: `
    <div class="user-management-container">
      <!-- Header -->
      <div class="header">
        <div class="header-left">
          <h1>User</h1>
          <p class="date-time">{{ currentDate | date: 'fullDate' }} - {{ currentTime | date: 'shortTime' }}</p>
        </div>
        <div class="header-right">
          <span class="admin-name">Tatva Admin</span>
          <button (click)="logout()" class="logout-btn">Logout</button>
        </div>
      </div>

      <!-- Search and Add Button -->
      <div class="toolbar">
        <div class="search-container">
          <input type="text" placeholder="Search" class="search-input" [(ngModel)]="searchTerm" (input)="filterUsers()">
        </div>
        <button (click)="openAddUserModal()" class="add-btn">
          <span class="add-icon">★</span> Add
        </button>
      </div>

      <!-- Users Table -->
      <div class="table-container">
        <table class="users-table">
          <thead>
            <tr>
              <th>First Name</th>
              <th>Last Name</th>
              <th>Email</th>
              <th>Phone Number</th>
              <th>Action</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let user of filteredUsers" [class.selected]="selectedUser?.id === user.id">
              <td>{{ getFirstName(user.name) }}</td>
              <td>{{ getLastName(user.name) }}</td>
              <td>{{ user.username }}</td>
              <td>{{ user.phoneNumber || 'N/A' }}</td>
              <td class="action-cell">
                <button (click)="editUser(user)" class="edit-btn" title="Edit" style="color: #667eea;">
                  EDIT
                </button>
                <button (click)="deleteUser(user)" class="delete-btn" title="Delete" style="color: #e74c3c;">
                  DELETE
                </button>
              </td>
            </tr>
          </tbody>
        </table>

        <div *ngIf="filteredUsers.length === 0" class="no-data">
          <p>No users found.</p>
        </div>
      </div>

      <!-- Pagination -->
      <div class="pagination">
        <button [disabled]="currentPage === 1" (click)="previousPage()">&lt;</button>
        <span class="page-info">{{ currentPage }}</span>
        <button [disabled]="currentPage === totalPages" (click)="nextPage()">&gt;</button>
      </div>

      <!-- Deleted Users Section -->
      <div class="deleted-users-section" *ngIf="showDeletedUsers">
        <h3>Deleted Users</h3>
        <div class="deleted-users-list">
          <div *ngFor="let user of deletedUsers" class="deleted-user-item">
            <span>{{ user.name }} ({{ user.username }})</span>
            <button (click)="restoreUser(user)" class="restore-btn">Restore</button>
          </div>
        </div>
      </div>

      <button (click)="toggleDeletedUsers()" class="toggle-deleted-btn">
        {{ showDeletedUsers ? 'Hide' : 'Show' }} Deleted Users
      </button>
    </div>

    <!-- Add/Edit User Modal -->
    <div class="modal-overlay" *ngIf="showModal" (click)="closeModal()">
      <div class="modal-content" (click)="$event.stopPropagation()">
        <div class="modal-header">
          <h2>{{ isEditing ? 'Edit User' : 'Add New User' }}</h2>
          <button class="close-btn" (click)="closeModal()">×</button>
        </div>
        
        <form [formGroup]="userForm" (ngSubmit)="saveUser()">
          <div class="form-row">
            <div class="form-group">
              <label for="username">Username/Email *</label>
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
              <label for="name">Full Name *</label>
              <input 
                type="text" 
                id="name" 
                formControlName="name"
                [class.error]="userForm.get('name')?.invalid && userForm.get('name')?.touched"
              >
              <div class="error-message" *ngIf="userForm.get('name')?.invalid && userForm.get('name')?.touched">
                Name is required
              </div>
            </div>
          </div>

          <div class="form-row">
            <div class="form-group">
              <label for="phoneNumber">Phone Number</label>
              <input 
                type="tel" 
                id="phoneNumber" 
                formControlName="phoneNumber"
              >
            </div>

            <div class="form-group">
              <label for="roleId">Role *</label>
              <select id="roleId" formControlName="roleId">
                <option value="1">Admin</option>
                <option value="2">User</option>
              </select>
            </div>
          </div>

          <div class="form-row" *ngIf="!isEditing">
            <div class="form-group">
              <label for="password">Password *</label>
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
          </div>

          <div class="form-actions">
            <button type="button" (click)="closeModal()" class="cancel-btn">Cancel</button>
            <button type="submit" [disabled]="userForm.invalid || loading" class="save-btn">
              {{ loading ? 'Saving...' : (isEditing ? 'Update User' : 'Add User') }}
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- Success/Error Messages -->
    <div class="toast" *ngIf="successMessage" [class.show]="successMessage">
      {{ successMessage }}
    </div>
    <div class="toast error" *ngIf="errorMessage" [class.show]="errorMessage">
      {{ errorMessage }}
    </div>
  `,
  styles: [`
    .user-management-container {
      padding: 20px;
      background: white;
      min-height: 100vh;
    }

    .header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      background: white;
      padding: 15px 20px;
      border-radius: 10px;
      margin-bottom: 20px;
      box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
    }

    .header h1 {
      margin: 0;
      font-size: 24px;
      color: #333;
    }

    .date-time {
      font-size: 0.875rem;
      color: #666;
      margin-top: 5px;
    }

    .header-right {
      display: flex;
      align-items: center;
      gap: 15px;
    }

    .admin-name {
      color: #666;
    }

    .logout-btn {
      background: #e74c3c;
      color: white;
      border: none;
      padding: 0.75rem 1rem;
      border-radius: 5px;
      cursor: pointer;
      font-size: 1rem;
      transition: background 0.3s;
    }

    .logout-btn:hover {
      background: #c0392b;
    }

    .toolbar {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 20px;
    }

    .search-container {
      position: relative;
    }

    .search-input {
      padding: 0.75rem;
      border: 1px solid #ddd;
      border-radius: 5px;
      width: 300px;
      font-size: 1rem;
      box-sizing: border-box;
    }

    .add-btn {
      background: #667eea;
      color: white;
      border: none;
      padding: 0.75rem 1rem;
      border-radius: 5px;
      cursor: pointer;
      display: flex;
      align-items: center;
      gap: 8px;
      font-size: 1rem;
      transition: background 0.3s;
    }

    .add-btn:hover {
      background: #5a6fd8;
    }

    .add-icon {
      font-size: 16px;
    }

    .table-container {
      background: white;
      border-radius: 10px;
      overflow: hidden;
      box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
    }

    .users-table {
      width: 100%;
      border-collapse: collapse;
    }

    .users-table th {
      background: #f8f9fa;
      padding: 15px;
      text-align: left;
      font-weight: bold;
      color: #555;
      border-bottom: 1px solid #ddd;
    }

    .users-table td {
      padding: 15px;
      border-bottom: 1px solid #eee;
    }

    .users-table tr:hover {
      background: #f8f9fa;
    }

    .users-table tr.selected {
      background: #e6effe;
    }

    .action-cell {
      display: flex;
      gap: 8px;
    }

    .edit-btn, .delete-btn {
      background: none;
      border: none;
      cursor: pointer;
      padding: 5px;
      border-radius: 5px;
      font-size: 16px;
      transition: background 0.3s;
    }

    .edit-btn:hover {
      background: #e6effe;
    }

    .delete-btn:hover {
      background: #ffebee;
    }

    .pagination {
      display: flex;
      justify-content: center;
      align-items: center;
      gap: 10px;
      margin: 20px 0;
    }

    .pagination button {
      background: #667eea;
      color: white;
      border: none;
      padding: 0.75rem;
      border-radius: 5px;
      cursor: pointer;
      font-size: 1rem;
      transition: background 0.3s;
    }

    .pagination button:hover:not(:disabled) {
      background: #5a6fd8;
    }

    .pagination button:disabled {
      background: #ccc;
      cursor: not-allowed;
    }

    .page-info {
      background: #667eea;
      color: white;
      padding: 0.75rem;
      border-radius: 5px;
      min-width: 30px;
      text-align: center;
      font-size: 1rem;
    }

    .deleted-users-section {
      background: white;
      padding: 20px;
      border-radius: 10px;
      margin-top: 20px;
      box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
    }

    .deleted-users-section h3 {
      margin-top: 0;
      color: #e74c3c;
    }

    .deleted-user-item {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 10px 0;
      border-bottom: 1px solid #eee;
    }

    .restore-btn {
      background: #27ae60;
      color: white;
      border: none;
      padding: 0.75rem;
      border-radius: 5px;
      cursor: pointer;
      font-size: 1rem;
      transition: background 0.3s;
    }

    .restore-btn:hover {
      background: #219a52;
    }

    .toggle-deleted-btn {
      background: #6c757d;
      color: white;
      border: none;
      padding: 0.75rem 1rem;
      border-radius: 5px;
      cursor: pointer;
      margin-top: 20px;
      font-size: 1rem;
      transition: background 0.3s;
    }

    .toggle-deleted-btn:hover {
      background: #5a6268;
    }

    .modal-overlay {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: rgba(0,0,0,0.5);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 1000;
    }

    .modal-content {
      background: white;
      border-radius: 10px;
      padding: 0;
      width: 90%;
      max-width: 600px;
      max-height: 90vh;
      overflow-y: auto;
      box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
    }

    .modal-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 20px;
      border-bottom: 1px solid #eee;
    }

    .modal-header h2 {
      margin: 0;
      color: #333;
    }

    .close-btn {
      background: none;
      border: none;
      font-size: 24px;
      cursor: pointer;
      color: #666;
      transition: color 0.3s;
    }

    .close-btn:hover {
      color: #333;
    }

    .modal-content form {
      padding: 20px;
    }

    .form-row {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 20px;
      margin-bottom: 20px;
    }

    .form-group {
      display: flex;
      flex-direction: column;
    }

    .form-group label {
      margin-bottom: 0.5rem;
      font-weight: bold;
      color: #555;
    }

    .form-group input,
    .form-group select {
      padding: 0.75rem;
      border: 1px solid #ddd;
      border-radius: 5px;
      font-size: 1rem;
      box-sizing: border-box;
    }

    .form-group input.error {
      border-color: #e74c3c;
    }

    .error-message {
      color: #e74c3c;
      font-size: 0.875rem;
      margin-top: 0.5rem;
    }

    .form-actions {
      display: flex;
      gap: 10px;
      justify-content: flex-end;
      padding-top: 20px;
      border-top: 1px solid #eee;
    }

    .cancel-btn {
      background: #6c757d;
      color: white;
      border: none;
      padding: 0.75rem 1rem;
      border-radius: 5px;
      cursor: pointer;
      font-size: 1rem;
      transition: background 0.3s;
    }

    .cancel-btn:hover {
      background: #5a6268;
    }

    .save-btn {
      background: #667eea;
      color: white;
      border: none;
      padding: 0.75rem 1rem;
      border-radius: 5px;
      cursor: pointer;
      font-size: 1rem;
      transition: background 0.3s;
    }

    .save-btn:hover:not(:disabled) {
      background: #5a6fd8;
    }

    .save-btn:disabled {
      background: #ccc;
      cursor: not-allowed;
    }

    .no-data {
      text-align: center;
      padding: 40px;
      color: #666;
    }

    .toast {
      position: fixed;
      top: 20px;
      right: 20px;
      background: #27ae60;
      color: white;
      padding: 15px 20px;
      border-radius: 5px;
      transform: translateX(100%);
      transition: transform 0.3s ease;
      z-index: 1001;
    }

    .toast.error {
      background: #e74c3c;
    }

    .toast.show {
      transform: translateX(0);
    }

    @media (max-width: 768px) {
      .form-row {
        grid-template-columns: 1fr;
      }
      
      .toolbar {
        flex-direction: column;
        gap: 15px;
        align-items: stretch;
      }
      
      .search-input {
        width: 100%;
      }
    }
  `]
})
export class DashboardComponent implements OnInit {
  userForm: FormGroup;
  users: User[] = [];
  deletedUsers: User[] = [];
  filteredUsers: User[] = [];
  selectedUser: User | null = null;
  loading = false;
  successMessage = '';
  errorMessage = '';
  showModal = false;
  isEditing = false;
  showDeletedUsers = false;
  searchTerm = '';
  currentPage = 1;
  totalPages = 1;
  
  private apiUrl = 'https://localhost:7089/api'; 
  currentDate = new Date();
  currentTime = new Date();


  constructor(
    private formBuilder: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private http: HttpClient
  ) {
    this.userForm = this.formBuilder.group({
      username: ['', [Validators.required, Validators.email]],
      name: ['', [Validators.required]],
      phoneNumber: [''],
      password: ['', [Validators.required, Validators.minLength(6)]],
      roleId: [2, Validators.required]
    });
  }

  ngOnInit() {
    this.loadUsers();
    this.loadDeletedUsers();
  }

  private getAuthHeaders() {
    const token = this.getTokenFromCurrentUser();
    return new HttpHeaders({
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    });
  }

 
  private getTokenFromCurrentUser(): string {
    const currentUser = this.authService.currentUserValue;
    
    return (currentUser as any)?.token || (currentUser as any)?.accessToken || '';
  }

  loadUsers() {
    this.http.get<User[]>(`${this.apiUrl}/user`, { headers: this.getAuthHeaders() })
      .subscribe({
        next: (users) => {
          this.users = users;
          this.filterUsers();
        },
        error: (error) => {
          this.showError('Failed to load users');
          console.error('Error loading users:', error);
        }
      });
  }

  loadDeletedUsers() {
    this.http.get<User[]>(`${this.apiUrl}/user/deleted`, { headers: this.getAuthHeaders() })
      .subscribe({
        next: (users) => {
          this.deletedUsers = users;
        },
        error: (error) => {
          console.error('Error loading deleted users:', error);
        }
      });
  }

  filterUsers() {
    if (!this.searchTerm.trim()) {
      this.filteredUsers = [...this.users];
    } else {
      const term = this.searchTerm.toLowerCase();
      this.filteredUsers = this.users.filter(user => 
        user.name.toLowerCase().includes(term) ||
        user.username.toLowerCase().includes(term) ||
        (user.phoneNumber && user.phoneNumber.includes(term))
      );
    }
  }

  openAddUserModal() {
    this.isEditing = false;
    this.showModal = true;
    this.userForm.reset();
    this.userForm.patchValue({ roleId: 2 });
    this.userForm.get('password')?.setValidators([Validators.required, Validators.minLength(6)]);
    this.userForm.get('password')?.updateValueAndValidity();
  }

  editUser(user: User) {
    this.isEditing = true;
    this.selectedUser = user;
    this.showModal = true;
    
    this.userForm.patchValue({
      username: user.username,
      name: user.name,
      phoneNumber: user.phoneNumber,
      roleId: user.roleId
    });
    
    
    this.userForm.get('password')?.clearValidators();
    this.userForm.get('password')?.updateValueAndValidity();
  }

  saveUser() {
    if (this.userForm.invalid) {
      return;
    }

    this.loading = true;
    
    if (this.isEditing && this.selectedUser) {
      this.updateUser();
    } else {
      this.createUser();
    }
  }

  createUser() {
    const userData = {
      username: this.userForm.value.username,
      name: this.userForm.value.name,
      phoneNumber: this.userForm.value.phoneNumber,
      password: this.userForm.value.password,
      roleId: this.userForm.value.roleId
    };

    this.http.post<User>(`${this.apiUrl}/user`, userData, { headers: this.getAuthHeaders() })
      .subscribe({
        next: (user) => {
          this.loading = false;
          this.showSuccess('User created successfully!');
          this.closeModal();
          this.loadUsers();
        },
        error: (error) => {
          this.loading = false;
          this.showError(error.error?.message || 'Failed to create user');
        }
      });
  }

  updateUser() {
    if (!this.selectedUser) return;

    const userData = {
      id: this.selectedUser.id,
      username: this.userForm.value.username,
      name: this.userForm.value.name,
      phoneNumber: this.userForm.value.phoneNumber,
      password: this.userForm.value.password || null,
      roleId: this.userForm.value.roleId
    };

    this.http.put(`${this.apiUrl}/user/${this.selectedUser.id}`, userData, { headers: this.getAuthHeaders() })
      .subscribe({
        next: () => {
          this.loading = false;
          this.showSuccess('User updated successfully!');
          this.closeModal();
          this.loadUsers();
        },
        error: (error) => {
          this.loading = false;
          this.showError(error.error?.message || 'Failed to update user');
        }
      });
  }

  deleteUser(user: User) {
    if (confirm(`Are you sure you want to delete ${user.name}?`)) {
      this.http.delete(`${this.apiUrl}/user/${user.id}`, { headers: this.getAuthHeaders() })
        .subscribe({
          next: () => {
            this.showSuccess('User deleted successfully!');
            this.loadUsers();
            this.loadDeletedUsers();
          },
          error: (error) => {
            this.showError(error.error?.message || 'Failed to delete user');
          }
        });
    }
  }

  restoreUser(user: User) {
    if (confirm(`Are you sure you want to restore ${user.name}?`)) {
      this.http.post(`${this.apiUrl}/user/restore/${user.id}`, {}, { headers: this.getAuthHeaders() })
        .subscribe({
          next: () => {
            this.showSuccess('User restored successfully!');
            this.loadUsers();
            this.loadDeletedUsers();
          },
          error: (error) => {
            this.showError(error.error?.message || 'Failed to restore user');
          }
        });
    }
  }

  closeModal() {
    this.showModal = false;
    this.isEditing = false;
    this.selectedUser = null;
    this.userForm.reset();
  }

  toggleDeletedUsers() {
    this.showDeletedUsers = !this.showDeletedUsers;
    if (this.showDeletedUsers) {
      this.loadDeletedUsers();
    }
  }

  getFirstName(fullName: string): string {
    return fullName.split(' ')[0] || '';
  }

  getLastName(fullName: string): string {
    const parts = fullName.split(' ');
    return parts.length > 1 ? parts.slice(1).join(' ') : '';
  }

  previousPage() {
    if (this.currentPage > 1) {
      this.currentPage--;
    }
  }

  nextPage() {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
    }
  }

  showSuccess(message: string) {
    this.successMessage = message;
    setTimeout(() => this.successMessage = '', 3000);
  }

  showError(message: string) {
    this.errorMessage = message;
    setTimeout(() => this.errorMessage = '', 5000);
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}