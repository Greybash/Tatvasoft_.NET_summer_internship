import { Component, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NavbarComponent } from '../navbar/navbar.component';
import { firstValueFrom } from 'rxjs';

// Updated interface to match actual API response
interface User {
  name: any;
  username: string;
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  // Add other user properties as needed
}

interface Mission {
  id: number;
  missionTitle: string;
  missionDescription: string;
  missionOrganisationName: string;
  missionOrganisationDetail: string;
  countryId: number;
  cityId: number;
  missionThemeId: number;
  startDate: string;
  endDate: string;
  missionType: string;
  totalSheets: number;
  registrationDeadLine: string;
  missionSkillId: number;
  missionImages: string;
  missionDocuments?: string;
  missionAvailability: string;
  missionVideoUrl: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
  missionApps?: any[];
}

interface AdminApplicationResponse {
  id: number;
  missionId: number;
  userId: number;
  appliedDate: string;
  status: boolean; // Note: API returns boolean, not string
  applystatus: boolean;
  seats: number;
  mission: Mission;
  user: User; // Add user data
}

// For display purposes, we'll create a flattened interface
interface DisplayApplication {
  id: number;
  missionId: number;
  missionTitle: string;
  userId: number;
  userName: string; // Will need to be fetched or set to placeholder
  userEmail: string; // Will need to be fetched or set to placeholder
  appliedDate: string;
  status: string;
  seats: number;
  missionStartDate: string;
  missionEndDate?: string;
}

interface PaginatedApplicationResponse {
  applications: DisplayApplication[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

interface ApplicationStatistics {
  totalApplications: number;
  pendingApplications: number;
  approvedApplications: number;
  totalSeatsApplied: number;
  totalSeatsApproved: number;
}

interface AdminActionRequest {
  comments?: string;
}

@Component({
  selector: 'app-missionapplication',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, NavbarComponent],
  templateUrl: './missionapplication.component.html',
  styleUrl: './missionapplication.component.css'
})
export class MissionapplicationComponent implements OnInit {
  applications: DisplayApplication[] = [];
  filteredApplications: DisplayApplication[] = [];
  statistics: ApplicationStatistics | null = null;
  
  // Pagination
  currentPage = 1;
  pageSize = 10;
  totalCount = 0;
  totalPages = 0;
  
  // Filters
  searchTerm = '';
  statusFilter = '';
  missionIdFilter: number | null = null;
  
  // UI State
  loading = false;
  error = '';
  showModal = false;
  showStatistics = true;
  
  // Modal state
  selectedApplication: DisplayApplication | null = null;
  actionForm: FormGroup;
  actionType: 'approve' | 'reject' | null = null;
  
  // Date
  currentDate = new Date();
  
  // API base URL
  private apiUrl = 'https://localhost:7089/api/MissionApplication';
  
  constructor(
    private http: HttpClient,
    private fb: FormBuilder
  ) {
    this.actionForm = this.fb.group({
      comments: ['', [Validators.maxLength(500)]]
    });
  }
  
  ngOnInit(): void {
    this.loadApplications();
    this.loadStatistics();
  }
  
  // HTTP Headers with auth
  private getHttpHeaders(): HttpHeaders {
    const token = localStorage.getItem('authToken');
    return new HttpHeaders({
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    });
  }
  
  // Transform API response to display format
  private transformApiResponse(apiResponse: AdminApplicationResponse[]): DisplayApplication[] {
    return apiResponse.map(app => ({
      id: app.id,
      missionId: app.missionId,
      missionTitle: app.mission.missionTitle,
      userId: app.userId,
      userName: `${app.user.name}`.trim() || `User ${app.userId}`,
      userEmail: app.user.username || `user${app.userId}@example.com`,
      appliedDate: app.appliedDate,
      status: app.status ? 'approved' : 'pending',
      seats: app.seats,
      missionStartDate: app.mission.startDate,
      missionEndDate: app.mission.endDate
    }));
  }
  
  // Load applications with filters
  async loadApplications(): Promise<void> {
    this.loading = true;
    this.error = '';
    
    try {
      const params = new URLSearchParams({
        page: this.currentPage.toString(),
        pageSize: this.pageSize.toString()
      });
      
      if (this.statusFilter) {
        params.append('status', this.statusFilter);
      }
      
      if (this.missionIdFilter) {
        params.append('missionId', this.missionIdFilter.toString());
      }
      
      const response = await firstValueFrom(
        this.http.get<AdminApplicationResponse[]>(
          `${this.apiUrl}/admin/all?${params.toString()}`,
          { headers: this.getHttpHeaders() }
        )
      );
      
      if (response) {
        // Transform the response to match our display interface
        const transformedApplications = this.transformApiResponse(response);
        this.applications = transformedApplications;
        
        // Since API doesn't return pagination info, we'll calculate it
        this.totalCount = transformedApplications.length;
        this.totalPages = Math.ceil(this.totalCount / this.pageSize);
        
        this.filterApplications();
      }
    } catch (error: any) {
      this.error = error?.error?.message || 'Failed to load applications';
      console.error('Error loading applications:', error);
    } finally {
      this.loading = false;
    }
  }
  
  // Load pending applications
  async loadPendingApplications(): Promise<void> {
    this.loading = true;
    this.error = '';
    
    try {
      const params = new URLSearchParams({
        page: this.currentPage.toString(),
        pageSize: this.pageSize.toString()
      });
      
      const response = await firstValueFrom(
        this.http.get<AdminApplicationResponse[]>(
          `${this.apiUrl}/admin/pending?${params.toString()}`,
          { headers: this.getHttpHeaders() }
        )
      );
      
      if (response) {
        const transformedApplications = this.transformApiResponse(response);
        this.applications = transformedApplications;
        this.filterApplications();
      }
    } catch (error: any) {
      this.error = error?.error?.message || 'Failed to load pending applications';
      console.error('Error loading pending applications:', error);
    } finally {
      this.loading = false;
    }
  }
  
  // Load statistics
  async loadStatistics(): Promise<void> {
    try {
      const response = await firstValueFrom(
        this.http.get<ApplicationStatistics>(
          `${this.apiUrl}/admin/statistics`,
          { headers: this.getHttpHeaders() }
        )
      );
      
      if (response) {
        this.statistics = response;
      }
    } catch (error: any) {
      console.error('Error loading statistics:', error);
      // Don't show error for statistics as it's not critical
    }
  }
  
  // Filter applications based on search term
  filterApplications(): void {
    if (!this.searchTerm.trim()) {
      this.filteredApplications = [...this.applications];
    } else {
      const term = this.searchTerm.toLowerCase();
      this.filteredApplications = this.applications.filter(app =>
        app.missionTitle.toLowerCase().includes(term) ||
        app.userName.toLowerCase().includes(term) ||
        app.userEmail.toLowerCase().includes(term)
      );
    }
  }
  
  // Pagination methods
  previousPage(): void {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.loadApplications();
    }
  }
  
  nextPage(): void {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
      this.loadApplications();
    }
  }
  
  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages && page !== this.currentPage) {
      this.currentPage = page;
      this.loadApplications();
    }
  }
  
  getPageNumbers(): number[] {
    const pages: number[] = [];
    const maxVisible = 5;
    
    let start = Math.max(1, this.currentPage - Math.floor(maxVisible / 2));
    let end = Math.min(this.totalPages, start + maxVisible - 1);
    
    if (end - start + 1 < maxVisible && start > 1) {
      start = Math.max(1, end - maxVisible + 1);
    }
    
    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    
    return pages;
  }
  
  // Filter methods
  onStatusFilterChange(): void {
    this.currentPage = 1;
    this.loadApplications();
  }
  
  onMissionFilterChange(): void {
    this.currentPage = 1;
    this.loadApplications();
  }
  
  clearFilters(): void {
    this.statusFilter = '';
    this.missionIdFilter = null;
    this.searchTerm = '';
    this.currentPage = 1;
    this.loadApplications();
  }
  
  // Modal methods
  openApproveModal(application: DisplayApplication): void {
    this.selectedApplication = application;
    this.actionType = 'approve';
    this.actionForm.reset();
    this.showModal = true;
    this.error = '';
  }
  
  openRejectModal(application: DisplayApplication): void {
    this.selectedApplication = application;
    this.actionType = 'reject';
    this.actionForm.reset();
    this.showModal = true;
    this.error = '';
  }
  
  closeModal(): void {
    this.showModal = false;
    this.selectedApplication = null;
    this.actionType = null;
    this.actionForm.reset();
    this.error = '';
  }
  
  // Action methods
  async processApplication(): Promise<void> {
    if (!this.selectedApplication || !this.actionType) return;
    
    this.loading = true;
    this.error = '';
    
    try {
      const request: AdminActionRequest = {
        comments: this.actionForm.get('comments')?.value || undefined
      };
      
      const endpoint = this.actionType === 'approve' ? 'approve' : 'reject';
      const url = `${this.apiUrl}/admin/${endpoint}/${this.selectedApplication.id}`;
      
      await firstValueFrom(
        this.http.put(url, request, {
          headers: this.getHttpHeaders()
        })
      );
      
      // Reload data
      await this.loadApplications();
      await this.loadStatistics();
      
      this.closeModal();
      
    } catch (error: any) {
      this.error = error?.error?.message || `Failed to ${this.actionType} application`;
      console.error(`Error ${this.actionType}ing application:`, error);
    } finally {
      this.loading = false;
    }
  }
  
  // Utility methods
  trackByFn(index: number, item: DisplayApplication): number {
    return item.id;
  }
  
  getStatusClass(status: string): string {
    return status.toLowerCase() === 'approved' ? 'status-approved' : 'status-pending';
  }
  
  getStatusText(status: string): string {
    return status === 'true' || status === '1' || status.toLowerCase() === 'approved' 
      ? 'Approved' 
      : 'Pending';
  }
  
  formatDate(dateString: string): string {
    try {
      return new Date(dateString).toLocaleDateString();
    } catch {
      return 'Invalid Date';
    }
  }
  
  formatDateTime(dateString: string): string {
    try {
      return new Date(dateString).toLocaleString();
    } catch {
      return 'Invalid Date';
    }
  }
  
  getErrorMessage(fieldName: string): string {
    const field = this.actionForm.get(fieldName);
    if (field?.errors && field.touched) {
      if (field.errors['required']) return `${fieldName} is required`;
      if (field.errors['maxlength']) {
        return `${fieldName} must be less than ${field.errors['maxlength'].requiredLength} characters`;
      }
    }
    return '';
  }
  
  // Math utility for template
  Math = Math;
}