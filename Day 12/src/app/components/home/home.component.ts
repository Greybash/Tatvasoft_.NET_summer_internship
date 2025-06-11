import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MissionService, Mission, MissionFilters } from '../../services/mission.service';
import { MissionApplication, MissionApplicationRequest, MissionApplicationService } from '../../services/missionapplication.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-mission-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  providers: [MissionService, AuthService, ],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {
  missions: Mission[] = [];
  totalCount: number = 0;
  currentPage: number = 1;
  pageSize: number = 9; // 3x3 grid
  totalPages: number = 0;
  isLoading: boolean = false;
  error: string = '';

  // Application related properties
  userApplications: MissionApplication[] = [];
  isApplying: { [missionId: number]: boolean } = {};
  currentUserId: number =0; // Changed to nullable
  
  // Application modal state
  showApplicationModal: boolean = false;
  selectedMission: Mission | null = null;
  applicationSeats: number = 1;

  // Base path for mission images
  private readonly imageBasePath = '';
  
  filters: MissionFilters = {
    page: 1,
    pageSize: 9,
    sortBy: 'createdAt',
    sortDescending: true,
    isActive: true
  };

  // Search and filter options
  searchTitle: string = '';
  selectedSortBy: string = 'createdAt';
  sortDescending: boolean = true;

  sortOptions = [
    { value: 'createdAt', label: 'Date Created' },
    { value: 'title', label: 'Title' },
    { value: 'startDate', label: 'Start Date' },
    { value: 'organisation', label: 'Organization' },
    { value: 'missionType', label: 'Mission Type' }
  ];

  constructor(
    private missionService: MissionService,
    private missionApplicationService: MissionApplicationService,
    private authService: AuthService // Inject AuthService
  ) { }

ngOnInit() {
  console.log('HomeComponent ngOnInit called');
  
  const currentUser = this.authService.currentUserValue;
  console.log('AuthService currentUserValue:', currentUser);
  console.log('AuthService isAuthenticated:', this.authService.isAuthenticated());
  
  if (this.authService.isAuthenticated() && currentUser) {

    // Use the available user data instead of trying to get a separate userId
    console.log('User authenticated hiiiiiiiiiiiiiiiiiiiiiiiii:', currentUser);
    const token = currentUser.token;
this.currentUserId = parseInt(this.getNameIdentifierFromToken(token), 10);
console.log('Extracted user ID from token:', this.currentUserId);
    // Continue with authenticated user logic
  } else {
    console.log('User not authenticated');
    // Handle unauthenticated state
  }

  this.loadMissions();
  this.loadUserApplications();
}


private getNameIdentifierFromToken(token: string): string {
  try {
    const payloadBase64 = token.split('.')[1]; // Get middle part of JWT
    const decodedJson = atob(payloadBase64);   // Decode base64 to string
    const decoded = JSON.parse(decodedJson);   // Parse to object

    return decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"] || '';
  } catch (error) {
    console.error('Failed to decode token:', error);
    return '';
  }
}
  /**
   * Load missions with current filters
   */
  loadMissions(): void {
    this.isLoading = true;
    this.error = '';

    // Update filters with case-insensitive search
    this.filters = {
      ...this.filters,
      title: this.searchTitle ? this.searchTitle.toLowerCase() : undefined,
      sortBy: this.selectedSortBy,
      sortDescending: this.sortDescending,
      page: this.currentPage,
      pageSize: this.pageSize
    };

    this.missionService.getMissions(this.filters).subscribe({
      next: (response: { data: Mission[]; totalCount: number; totalPages: number; }) => {
        this.missions = response.data;
        this.totalCount = response.totalCount;
        this.totalPages = response.totalPages;
        this.isLoading = false;
      },
      error: (error: any) => {
        console.error('Error loading missions:', error);
        this.error = 'Failed to load missions. Please try again.';
        this.isLoading = false;
      }
    });
  }

  /**
   * Load user's applications
   */
  loadUserApplications(): void {
    this.missionApplicationService.getMyApplications().subscribe({
      next: (applications: MissionApplication[]) => {
        this.userApplications = applications;
      },
      error: (error: any) => {
        console.error('Error loading user applications:', error);
      }
    });
  }

  /**
   * Check if user has already applied for a mission
   */
  hasUserApplied(missionId: number): boolean {
  return this.userApplications.some(app => app.missionId === missionId);
}

  /**
   * Get user's application for a specific mission
   */
  getUserApplication(missionId: number): MissionApplication | undefined {
  return this.userApplications.find(app => app.missionId === missionId);
}


  /**
   * Handle search with case-insensitive filtering
   */
  onSearch(): void {
    this.currentPage = 1;
    this.loadMissions();
  }

  /**
   * Handle sort change
   */
  onSortChange(): void {
    this.currentPage = 1;
    this.loadMissions();
  }

  /**
   * Handle page change
   */
  onPageChange(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.loadMissions();
    }
  }

  /**
   * Get the first image for a mission with custom path
   */
  getFirstImage(mission: Mission): string {
    // Option 2: If mission has imageUrl property
    if (mission.missionImages) {
      return `${this.imageBasePath}${mission.missionImages}`;
    }
    
    // Option 3: Use mission ID to construct filename (if following naming convention)
    if (mission.id) {
      return `${this.imageBasePath}mission_${mission.id}.jpg`;
    }
    
    // Fallback to default image
    return '/assets/images/default-mission.jpg';
  }

  /**
   * Get full image URL (alternative method if serving from backend)
   */
  getImageUrl(imageName: string): string {
    if (!imageName) {
      return '/assets/images/default-mission.jpg';
    }
    return `${this.imageBasePath}${imageName}`;
  }

  
  /**
   * Handle image load error
   */
  onImageError(event: any): void {
    event.target.src = '/assets/images/default-mission.jpg';
  }

  isApplicationApproved(missionId: number): boolean {
  const application = this.getUserApplication(missionId);
  return application ? application.status === true : false;
}

  /**
   * Format date for display
   */
  formatDate(dateString: string): string {
    return this.missionService.formatDate(dateString);
  }

  /**
   * Get mission status based on dates and active status
   */
  getMissionStatus(mission: Mission): string {
    const now = new Date();
    const startDate = new Date(mission.startDate);
    const endDate = new Date(mission.endDate ?? '');

    // Check if mission is active first
    if (!mission.isActive) {
      return 'inactive';
    }

    // Check date-based status
    if (now < startDate) {
      return 'upcoming';
    } else if (now >= startDate && now <= endDate) {
      return 'ongoing';
    } else {
      return 'completed';
    }
  }

  /**
   * Check if registration is open (based on mission active status and dates)
   */
  isRegistrationOpen(mission: Mission): boolean {
    if (!mission.isActive) {
      return false;
    }
    
    const now = new Date();
    const startDate = new Date(mission.startDate);
    
    // Registration is open if mission is active and hasn't started yet
    return now < startDate;
  }

  /**
   * Get status badge class
   */
  getStatusBadgeClass(mission: Mission): string {
    const status = this.getMissionStatus(mission);
    switch (status) {
      case 'upcoming':
        return 'status-upcoming';
      case 'ongoing':
        return 'status-ongoing';
      case 'completed':
        return 'status-completed';
      case 'inactive':
        return 'status-inactive';
      default:
        return 'status-default';
    }
  }

  /**
   * Get status text based on mission state
   */
  getStatusText(mission: Mission): string {
  
    const status = this.getMissionStatus(mission);
    switch (status) {
      case 'upcoming':
        return 'AVAILABLE';
      case 'ongoing':
        return 'ONGOING';
      case 'completed':
        return 'COMPLETED';
      case 'inactive':
        return 'INACTIVE';
      default:
        return 'AVAILABLE';
    }
  }

  /**
   * Generate page numbers for pagination
   */
  getPageNumbers(): number[] {
    const pages: number[] = [];
    const maxPagesToShow = 5;
    let startPage = Math.max(1, this.currentPage - Math.floor(maxPagesToShow / 2));
    let endPage = Math.min(this.totalPages, startPage + maxPagesToShow - 1);

    if (endPage - startPage + 1 < maxPagesToShow) {
      startPage = Math.max(1, endPage - maxPagesToShow + 1);
    }

    for (let i = startPage; i <= endPage; i++) {
      pages.push(i);
    }

    return pages;
  }

  /**
   * Clear search and reset filters
   */
  clearSearch(): void {
    this.searchTitle = '';
    this.selectedSortBy = 'createdAt';
    this.sortDescending = true;
    this.currentPage = 1;
    this.loadMissions();
  }

  /**
   * Handle mission card click
   */
  onMissionClick(mission: Mission): void {
    console.log('Mission clicked:', mission);
    // You can implement navigation to mission details page here
    // this.router.navigate(['/missions', mission.id]);
  }

  /**
   * Handle apply button click - Open application modal
   */
  onApplyClick(mission: Mission, event: Event): void {
    event.stopPropagation(); // Prevent card click
    
    // Check if registration is open
    if (!this.isRegistrationOpen(mission)) {
      console.log('Registration is closed for this mission');
      return;
    }

    // Check if user has already applied
    if (this.hasUserApplied(mission.id)) {
      console.log('User has already applied for this mission');
      return;
    }
    
    this.selectedMission = mission;
    this.applicationSeats = 1;
    this.showApplicationModal = true;
  }

  /**
   * Close application modal
   */
  closeApplicationModal(): void {
    this.showApplicationModal = false;
    this.selectedMission = null;
    this.applicationSeats = 1;
  }

  /**
   * Submit mission application
   */
  submitApplication(): void {
    if (!this.selectedMission) {
      return;
    }

    const missionId = this.selectedMission.id;
    this.isApplying[missionId] = true;

    const request: MissionApplicationRequest = {
      MissionId: missionId,
      Seats: this.applicationSeats
    };

    this.missionApplicationService.applyForMission(this.currentUserId, request).subscribe({
      next: (response: any) => {
        console.log('Application successful:', response);
        this.isApplying[missionId] = false;
        this.closeApplicationModal();
        
        // Reload user applications to update the UI
        this.loadUserApplications();
        
        // Show success message
        alert('Application submitted successfully!');
      },
      error: (error: { error: { errorMessage: any; message: any; }; }) => {
        console.error('Application failed:', error);
        this.isApplying[missionId] = false;
        
        // Show error message
        const errorMessage = error.error?.errorMessage || error.error?.message || 'Application failed. Please try again.';
        alert(errorMessage);
      }
    });
  }

  /**
   * Cancel application
   */
  cancelApplication(missionId: number, event: Event): void {
    event.stopPropagation(); // Prevent card click
    
    const application = this.getUserApplication(missionId);
    if (!application) {
      return;
    }

    if (confirm('Are you sure you want to cancel this application?')) {
      this.missionApplicationService.cancelApplication(application.id).subscribe({
        next: (response: any) => {
          console.log('Application cancelled:', response);
          
          // Reload user applications to update the UI
          this.loadUserApplications();
          
          // Show success message
          alert('Application cancelled successfully!');
        },
        error: (error: { error: { message: string; }; }) => {
          console.error('Failed to cancel application:', error);
          
          // Show error message
          const errorMessage = error.error?.message || 'Failed to cancel application. Please try again.';
          alert(errorMessage);
        }
      });
    }
  }

  /**
   * Check if apply button should be shown
   */
  shouldShowApplyButton(mission: Mission): boolean {
    return this.isRegistrationOpen(mission) && !this.hasUserApplied(mission.id);
  }

  /**
   * Check if cancel button should be shown
   */
  shouldShowCancelButton(mission: Mission): boolean {
    const application = this.getUserApplication(mission.id);
    return application !== undefined && !application.status; // Only show if not approved
  }

  /**
   * Get application status text
   */
  getApplicationStatusText(mission: Mission): string {
    debugger;
    const application = this.getUserApplication(mission.id);
    if (!application) {
      return '';
    }

    if (application.status) {
      return 'APPROVED';
    } else {
      return 'PENDING';
    }
  }

  /**
   * Get button text based on application state
   */
  getButtonText(mission: Mission): string {
    debugger;
    if (this.hasUserApplied(mission.id)) {
      const application = this.getUserApplication(mission.id);
      if (application?.status) {
        return 'APPROVED';
      } else {
        return 'PENDING';
      }
    }
    
    if (this.isApplying[mission.id]) {
      return 'APPLYING...';
    }
    
    return 'APPLY NOW';
  }

  /**
   * Get button class based on application state
   */
  getButtonClass(mission: Mission): string {
    if (this.hasUserApplied(mission.id)) {
      const application = this.getUserApplication(mission.id);
      if (application?.status) {
        return 'btn-approved';
      } else {
        return 'btn-pending';
      }
    }
    
    return 'btn-apply';
  }
}