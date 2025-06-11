import { Component, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <nav class="sidebar">
      <div class="sidebar-header">
        <span class="navigation-title">NAVIGATION</span>
      </div>
      <ul class="sidebar-menu">
        <li>
          <a routerLink="/dashboard" routerLinkActive="active" class="menu-item">
           
            <span class="menu-text">User</span>
          </a>
        </li>
        <li>
          <a routerLink="/mission" routerLinkActive="active" class="menu-item">
          
            <span class="menu-text">Mission</span>
          </a>
        </li>
        <li>
          <a routerLink="/mission-theme" routerLinkActive="active" class="menu-item">
           
            <span class="menu-text">Mission Theme</span>
          </a>
        </li>
        <li>
          <a routerLink="/mission-skills" routerLinkActive="active" class="menu-item">
           
            <span class="menu-text">Mission Skills</span>
          </a>
        </li>
        <li>
          <a routerLink="/mission-application" routerLinkActive="active" class="menu-item">
            
            <span class="menu-text">Mission Application</span>
          </a>
        </li>
      </ul>
    </nav>
  `,
  styles: [`
    .sidebar {
      position: fixed;
      left: 0;
      top: 0;
      width: 280px;
      height: 100vh;
      background: linear-gradient(135deg, #ff9a56 0%, #ff6b35 50%, #f12711 100%);
      color: white;
      z-index: 1000;
      overflow-y: auto;
      box-shadow: 2px 0 10px rgba(0,0,0,0.1);
    }

    .sidebar-header {
      padding: 25px 20px 20px 20px;
      border-bottom: 1px solid rgba(255,255,255,0.1);
    }

    .navigation-title {
      font-size: 0.85rem;
      font-weight: 600;
      letter-spacing: 1px;
      opacity: 0.8;
      text-transform: uppercase;
    }

    .sidebar-menu {
      list-style: none;
      padding: 0;
      margin: 0;
    }

    .sidebar-menu li {
      margin: 0;
    }

    .menu-item {
      display: flex;
      align-items: center;
      padding: 15px 20px;
      color: white;
      text-decoration: none;
      transition: all 0.3s ease;
      border-left: 3px solid transparent;
      font-weight: 500;
    }

    .menu-item:hover {
      background-color: rgba(255,255,255,0.1);
      border-left-color: rgba(255,255,255,0.5);
    }

    .menu-item.active {
      background-color: rgba(255,255,255,0.2);
      border-left-color: white;
      font-weight: 600;
    }

    .menu-icon {
      font-size: 1.2rem;
      margin-right: 12px;
      width: 24px;
      text-align: center;
    }

    .menu-text {
      font-size: 0.95rem;
    }

    /* Responsive design */
    @media (max-width: 768px) {
      .sidebar {
        width: 260px;
        transform: translateX(-100%);
        transition: transform 0.3s ease;
      }

      .sidebar.mobile-open {
        transform: translateX(0);
      }
    }

    @media (max-width: 480px) {
      .sidebar {
        width: 100%;
      }
    }

    /* Scrollbar styling */
    .sidebar::-webkit-scrollbar {
      width: 4px;
    }

    .sidebar::-webkit-scrollbar-track {
      background: rgba(255,255,255,0.1);
    }

    .sidebar::-webkit-scrollbar-thumb {
      background: rgba(255,255,255,0.3);
      border-radius: 2px;
    }

    .sidebar::-webkit-scrollbar-thumb:hover {
      background: rgba(255,255,255,0.5);
    }
  `]
})
export class NavbarComponent {
  
}