import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { NavbarComponent } from '../navbar/navbar.component';


@Component({
  selector: 'app-mission-skill',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, NavbarComponent],
  templateUrl: './missionskills.component.html',
  styleUrls: ['./missionskills.component.css']
})
export class MissionSkillComponent implements OnInit {
  skills: any[] = [];
  filteredSkills: any[] = [];
  skillForm: FormGroup;
  showModal = false;
  isEditing = false;
  selectedSkill: any = null;
  searchTerm = '';
  currentPage = 1;
  pageSize = 10;
  totalPages = 1;
  totalCount = 0;
  loading = false;
  error = '';

  private apiUrl = 'https://localhost:7089/api/missionskill';
  currentDate: Date = new Date();

  constructor(private fb: FormBuilder, private http: HttpClient) {
    this.skillForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
      isActive: [true]
    });
  }

  ngOnInit(): void {
    this.loadSkills();
  }

  getAuthHeaders(): HttpHeaders {
    const token = localStorage.getItem('jwt');
    return new HttpHeaders({
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    });
  }

  loadSkills(): void {
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
        this.skills = res.data || res.Data || [];
        this.filteredSkills = this.skills;
        this.totalPages = res.totalPages || res.TotalPages || 1;
        this.totalCount = res.totalCount || res.TotalCount || 0;
        this.loading = false;
      },
      error: err => {
        console.error('Error loading skills', err);
        this.error = err.error?.message || 'Failed to load skills';
        this.loading = false;
      }
    });
  }

  openAddModal(): void {
    this.skillForm.reset({ 
      isActive: true,
      name: ''
    });
    this.isEditing = false;
    this.selectedSkill = null;
    this.showModal = true;
    this.error = '';
  }

  editSkill(skill: any): void {
    this.selectedSkill = skill;
    this.skillForm.patchValue({
      name: skill.name,
      isActive: skill.isActive
    });
    this.isEditing = true;
    this.showModal = true;
    this.error = '';
  }

  saveSkill(): void {
    if (this.skillForm.invalid) {
      this.markFormGroupTouched(this.skillForm);
      return;
    }

    this.loading = true;
    this.error = '';
    const skillData = this.skillForm.value;

    if (this.isEditing && this.selectedSkill) {
      this.http.put(`${this.apiUrl}/${this.selectedSkill.id}`, skillData, {
        headers: this.getAuthHeaders()
      }).subscribe({
        next: () => {
          this.loadSkills();
          this.closeModal();
          this.loading = false;
        },
        error: err => {
          console.error('Error updating skill', err);
          this.error = err.error?.message || 'Failed to update skill';
          this.loading = false;
        }
      });
    } else {
      this.http.post(this.apiUrl, skillData, {
        headers: this.getAuthHeaders()
      }).subscribe({
        next: () => {
          this.loadSkills();
          this.closeModal();
          this.loading = false;
        },
        error: err => {
          console.error('Error creating skill', err);
          this.error = err.error?.message || 'Failed to create skill';
          this.loading = false;
        }
      });
    }
  }

  deleteSkill(id: number): void {
    if (!confirm('Are you sure you want to delete this skill?')) return;

    this.loading = true;
    this.http.delete(`${this.apiUrl}/${id}`, {
      headers: this.getAuthHeaders()
    }).subscribe({
      next: () => {
        this.loadSkills();
        this.loading = false;
      },
      error: err => {
        console.error('Error deleting skill', err);
        this.error = err.error?.message || 'Failed to delete skill';
        this.loading = false;
      }
    });
  }

  filterSkills(): void {
    // Reset to first page when searching
    this.currentPage = 1;
    this.loadSkills();
  }

  closeModal(): void {
    this.showModal = false;
    this.isEditing = false;
    this.selectedSkill = null;
    this.skillForm.reset();
    this.error = '';
  }

  previousPage(): void {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.loadSkills();
    }
  }

  nextPage(): void {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
      this.loadSkills();
    }
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages && page !== this.currentPage) {
      this.currentPage = page;
      this.loadSkills();
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
    const control = this.skillForm.get(fieldName);
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