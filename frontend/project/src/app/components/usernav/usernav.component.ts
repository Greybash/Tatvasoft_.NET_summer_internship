import { Component, OnInit, HostListener, ElementRef } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common'; // Add this import

@Component({
  selector: 'app-user-nav',
  standalone: true, // Add this if using Angular 17+
  imports: [CommonModule], // Add this for standalone components
  template: `
    <div class="user-nav-container">
      <!-- Profile Button -->
      <button 
        class="profile-button"
        (click)="toggleDropdown()"
        [class.active]="isDropdownOpen">
        <i class="icon-user"></i>
        <span>Profile</span>
        <i class="icon-chevron-down" [class.rotated]="isDropdownOpen"></i>
      </button>

      <!-- Dropdown Menu -->
      <div 
        class="dropdown-menu" 
        [class.show]="isDropdownOpen"
        *ngIf="isDropdownOpen">
        <ul>
          <li>
            <button (click)="navigateToProfile()" class="dropdown-item">
              <i class="icon-user"></i>
              <span>View Profile</span>
            </button>
          </li>
          <li>
            <button (click)="openSettings()" class="dropdown-item">
              <i class="icon-settings"></i>
              <span>Settings</span>
            </button>
          </li>
          <li class="divider"></li>
          <li>
            <button (click)="logout()" class="dropdown-item logout">
              <i class="icon-logout"></i>
              <span>Logout</span>
            </button>
          </li>
        </ul>
      </div>
    </div>
  `,
  styles: [`
    .user-nav-container {
      position: relative;
      display: inline-block;
    }

    .profile-button {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 8px 12px;
      background-color: #f3f4f6;
      border: none;
      border-radius: 8px;
      cursor: pointer;
      transition: background-color 0.2s ease;
      font-size: 14px;
      font-weight: 500;
      color: #374151;
    }

    .profile-button:hover {
      background-color: #e5e7eb;
    }

    .profile-button:focus {
      outline: none;
      box-shadow: 0 0 0 2px #3b82f6;
    }

    .profile-button.active {
      background-color: #e5e7eb;
    }

    .icon-user::before {
      
      font-size: 16px;
    }

    .icon-chevron-down::before {
      
      font-size: 12px;
      transition: transform 0.2s ease;
    }

    .icon-chevron-down.rotated::before {
      transform: rotate(180deg);
    }

    .dropdown-menu {
      position: absolute;
      top: 100%;
      right: 0;
      margin-top: 8px;
      background: white;
      border: 1px solid #e5e7eb;
      border-radius: 8px;
      box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1);
      z-index: 1000;
      min-width: 200px;
      opacity: 0;
      transform: translateY(-10px);
      transition: opacity 0.2s ease, transform 0.2s ease;
    }

    .dropdown-menu.show {
      opacity: 1;
      transform: translateY(0);
    }

    .dropdown-menu ul {
      list-style: none;
      padding: 8px 0;
      margin: 0;
    }

    .dropdown-item {
      display: flex;
      align-items: center;
      gap: 12px;
      width: 100%;
      padding: 8px 16px;
      background: none;
      border: none;
      text-align: left;
      cursor: pointer;
      transition: background-color 0.15s ease;
      font-size: 14px;
      color: #374151;
    }

    .dropdown-item:hover {
      background-color: #f3f4f6;
    }

    .dropdown-item.logout {
      color: #dc2626;
    }

    .dropdown-item.logout:hover {
      background-color: #fef2f2;
    }

    .divider {
      height: 1px;
      background-color: #e5e7eb;
      margin: 4px 0;
    }

    .icon-settings::before {
      
      font-size: 16px;
    }

    .icon-logout::before {
      
      font-size: 16px;
    }
  `]
})
export class UserNavComponent implements OnInit {
  isDropdownOpen = false;

  constructor(
    private router: Router,
    private elementRef: ElementRef
  ) {}

  ngOnInit(): void {}

  toggleDropdown(): void {
    this.isDropdownOpen = !this.isDropdownOpen;
  }

  navigateToProfile(): void {
    this.isDropdownOpen = false;
    this.router.navigate(['/profile']);
  }

  openSettings(): void {
    this.isDropdownOpen = false;
    this.router.navigate(['/settings']);
  }

  logout(): void {
    this.isDropdownOpen = false;
    // Add your logout logic here
    console.log('Logging out...');
    // Example: Clear user session, navigate to login
    // this.authService.logout();
    this.router.navigate(['/login']);
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: Event): void {
    if (!this.elementRef.nativeElement.contains(event.target)) {
      this.isDropdownOpen = false;
    }
  }
}