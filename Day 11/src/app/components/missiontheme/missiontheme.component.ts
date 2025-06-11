import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { NavbarComponent } from '../navbar/navbar.component';

@Component({
  selector: 'app-mission-theme',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, NavbarComponent],
  templateUrl: './missiontheme.component.html',
  styleUrls: ['./missiontheme.component.css']
})
export class MissionThemeComponent implements OnInit {
  themes: any[] = [];
  filteredThemes: any[] = [];
  themeForm: FormGroup;
  showModal = false;
  isEditing = false;
  selectedTheme: any = null;
  searchTerm = '';
  currentPage = 1;
  pageSize = 10;
  totalPages = 1;
  totalCount = 0;
  loading = false;
  error = '';

  private apiUrl = 'https://localhost:7089/api/missiontheme';
  currentDate: Date = new Date();

  constructor(private fb: FormBuilder, private http: HttpClient) {
    this.themeForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
      isActive: [true]
    });
  }

  ngOnInit(): void {
    this.loadThemes();
  }

  getAuthHeaders(): HttpHeaders {
    const token = localStorage.getItem('jwt');
    return new HttpHeaders({
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    });
  }

  loadThemes(): void {
    this.loading = true;
    this.error = '';
    
    const params = new URLSearchParams({
      page: this.currentPage.toString(),
      pageSize: this.pageSize.toString(),
      sortBy: 'name',
      sortDescending: 'false'
    });

    // Add search filter if exists
    if (this.searchTerm) {
      params.append('name', this.searchTerm);
    }

    this.http.get<any>(`${this.apiUrl}?${params.toString()}`, {
      headers: this.getAuthHeaders()
    }).subscribe({
      next: res => {
        // Updated to match API response structure
        this.themes = res.data || res.Data || [];
        this.filteredThemes = this.themes;
        this.totalPages = res.totalPages || res.TotalPages || 1;
        this.totalCount = res.totalCount || res.TotalCount || 0;
        this.loading = false;
      },
      error: err => {
        console.error('Error loading themes', err);
        this.error = err.error?.message || 'Failed to load themes';
        this.loading = false;
      }
    });
  }

  openAddModal(): void {
    this.themeForm.reset({ 
      isActive: true,
      name: ''
    });
    this.isEditing = false;
    this.selectedTheme = null;
    this.showModal = true;
    this.error = '';
  }

  editTheme(theme: any): void {
    this.selectedTheme = theme;
    this.themeForm.patchValue({
      name: theme.name,
      isActive: theme.isActive
    });
    this.isEditing = true;
    this.showModal = true;
    this.error = '';
  }

  saveTheme(): void {
    if (this.themeForm.invalid) {
      this.markFormGroupTouched(this.themeForm);
      return;
    }

    this.loading = true;
    this.error = '';
    const themeData = this.themeForm.value;

    if (this.isEditing && this.selectedTheme) {
      this.http.put(`${this.apiUrl}/${this.selectedTheme.id}`, themeData, {
        headers: this.getAuthHeaders()
      }).subscribe({
        next: () => {
          this.loadThemes();
          this.closeModal();
          this.loading = false;
        },
        error: err => {
          console.error('Error updating theme', err);
          this.error = err.error?.message || 'Failed to update theme';
          this.loading = false;
        }
      });
    } else {
      this.http.post(this.apiUrl, themeData, {
        headers: this.getAuthHeaders()
      }).subscribe({
        next: () => {
          this.loadThemes();
          this.closeModal();
          this.loading = false;
        },
        error: err => {
          console.error('Error creating theme', err);
          this.error = err.error?.message || 'Failed to create theme';
          this.loading = false;
        }
      });
    }
  }

  deleteTheme(id: number): void {
    if (!confirm('Are you sure you want to delete this theme?')) return;

    this.loading = true;
    this.http.delete(`${this.apiUrl}/${id}`, {
      headers: this.getAuthHeaders()
    }).subscribe({
      next: () => {
        this.loadThemes();
        this.loading = false;
      },
      error: err => {
        console.error('Error deleting theme', err);
        this.error = err.error?.message || 'Failed to delete theme';
        this.loading = false;
      }
    });
  }

  filterThemes(): void {
    // Reset to first page when searching
    this.currentPage = 1;
    this.loadThemes();
  }

  closeModal(): void {
    this.showModal = false;
    this.isEditing = false;
    this.selectedTheme = null;
    this.themeForm.reset();
    this.error = '';
  }

  previousPage(): void {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.loadThemes();
    }
  }

  nextPage(): void {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
      this.loadThemes();
    }
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages && page !== this.currentPage) {
      this.currentPage = page;
      this.loadThemes();
    }
  }

  // Helper method to mark all form controls as touched for validation display
  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();
    });
  }

  // Helper method to get form control error message
  getErrorMessage(fieldName: string): string {
    const control = this.themeForm.get(fieldName);
    if (control?.errors && control.touched) {
      if (control.errors['required']) {
        return `${fieldName} is required`;
      }
      if (control.errors['minlength']) {
        return `${fieldName} must be at least ${control.errors['minlength'].requiredLength} characters`;
      }
      if (control.errors['maxlength']) {
        return `${fieldName} cannot exceed ${control.errors['maxlength'].requiredLength} characters`;
      }
    }
    return '';
  }

  // TrackBy function for better ngFor performance
  trackByFn(index: number, item: any): any {
    return item.id || index;
  }

  // Get page numbers for pagination
  getPageNumbers(): number[] {
    const pages: number[] = [];
    const start = Math.max(1, this.currentPage - 2);
    const end = Math.min(this.totalPages, this.currentPage + 2);
    
    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    
    return pages;
  }

  // Math object for template use
  Math = Math;
}