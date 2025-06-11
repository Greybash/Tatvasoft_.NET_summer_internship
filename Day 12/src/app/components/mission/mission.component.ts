import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { NavbarComponent } from '../navbar/navbar.component';


@Component({
  selector: 'app-mission',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, NavbarComponent],
  templateUrl: './mission.component.html',
  styleUrls: ['./mission.component.css']
})
export class MissionComponent implements OnInit {
  
  missions: any[] = [];
  filteredMissions: any[] = [];
  missionForm!: FormGroup;
  showModal = false;
  isEditing = false;
  selectedMission: any = null;
  searchTerm = '';
  currentPage = 1;
  pageSize = 10;
  totalPages = 1;
  totalCount = 0;
  loading = false;
  error = '';
  showFilters: boolean = true;

  // Dropdown data
  countries: any[] = [];
  cities: any[] = [];
  themes: any[] = [];
  skills: any[] = [];
  filteredCities: any[] = [];

  // Image upload
  selectedFiles: File[] = [];
  previewImages: string[] = [];
  uploadingImages = false;

  // Filter options
  filterForm!: FormGroup;
  missionTypes = ['TIME', 'GOAL'];
  availabilityOptions = ['FULL_TIME', 'PART_TIME', 'WEEKEND'];

  private apiUrl = 'https://localhost:7089/api';
  currentDate: Date = new Date();

  constructor(private fb: FormBuilder, private http: HttpClient) {
    this.initializeForms();
  }

  private initializeForms(): void {
    this.missionForm = this.fb.group({
      missionTitle: ['', [Validators.required, Validators.maxLength(200)]],
      missionDescription: ['', [Validators.required, Validators.maxLength(2000)]],
      missionOrganisationName: ['', [Validators.required, Validators.maxLength(200)]],
      missionOrganisationDetail: ['', [Validators.maxLength(1000)]],
      countryId: ['', [Validators.required]],
      cityId: ['', [Validators.required]],
      startDate: ['', [Validators.required]],
      endDate: ['', [Validators.required]],
      missionType: ['', [Validators.required]],
      totalSheets: [''],
      registrationDeadLine: [''],
      missionThemeId: ['', [Validators.required]],
      missionSkillId: ['', [Validators.required]],
      missionAvailability: [''],
      missionVideoUrl: ['', [Validators.maxLength(500)]],
      isActive: [true]
    });

    this.filterForm = this.fb.group({
      title: [''],
      organisationName: [''],
      countryId: [''],
      cityId: [''],
      themeId: [''],
      missionType: [''],
      startDateFrom: [''],
      startDateTo: [''],
      isActive: ['']
    });
  }

  ngOnInit(): void {
    this.loadMissions();
    this.loadCountries();
    this.loadThemes();
    this.loadSkills();
  }

  getAuthHeaders(): HttpHeaders {
    const token = localStorage.getItem('jwt');
    return new HttpHeaders({
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    });
  }

  getAuthHeadersForFormData(): HttpHeaders {
    const token = localStorage.getItem('jwt');
    return new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });
  }

  loadMissions(): void {
    this.loading = true;
    this.error = '';
    
    const params = new URLSearchParams({
      page: this.currentPage.toString(),
      pageSize: this.pageSize.toString(),
      sortBy: 'createdAt',
      sortDescending: 'true'
    });

    // Add filter parameters
    const filterValues = this.filterForm.value;
    Object.keys(filterValues).forEach(key => {
      if (filterValues[key] && filterValues[key] !== '') {
        params.append(key, filterValues[key]);
      }
    });

    this.http.get<any>(`${this.apiUrl}/mission?${params.toString()}`, {
      headers: this.getAuthHeaders()
    }).subscribe({
      next: res => {
        this.missions = res.data || res.Data || [];
        this.filteredMissions = this.missions;
        this.totalPages = res.totalPages || res.TotalPages || 1;
        this.totalCount = res.totalCount || res.TotalCount || 0;
        this.loading = false;
      },
      error: err => {
        console.error('Error loading missions', err);
        this.error = err.error?.message || 'Failed to load missions';
        this.loading = false;
      }
    });
  }

  loadCountries(): void {
    this.http.get<any[]>(`${this.apiUrl}/location/countries`, {
      headers: this.getAuthHeaders()
    }).subscribe({
      next: countries => {
        this.countries = countries;
      },
      error: err => {
        console.error('Error loading countries', err);
      }
    });
  }

  loadCitiesByCountry(countryId: number): void {
    if (!countryId) {
      this.cities = [];
      this.filteredCities = [];
      return;
    }

    this.http.get<any[]>(`${this.apiUrl}/location/countries/${countryId}/cities`, {
      headers: this.getAuthHeaders()
    }).subscribe({
      next: cities => {
        this.cities = cities;
        this.filteredCities = cities;
      },
      error: err => {
        console.error('Error loading cities', err);
      }
    });
  }

  loadThemes(): void {
    this.http.get<any>(`${this.apiUrl}/missiontheme?pageSize=1000`, {
      headers: this.getAuthHeaders()
    }).subscribe({
      next: res => {
        this.themes = res.data || res.Data || [];
      },
      error: err => {
        console.error('Error loading themes', err);
      }
    });
  }

  loadSkills(): void {
    this.http.get<any>(`${this.apiUrl}/missionskill?pageSize=1000`, {
      headers: this.getAuthHeaders()
    }).subscribe({
      next: res => {
        this.skills = res.data || res.Data || [];
      },
      error: err => {
        console.error('Error loading skills', err);
      }
    });
  }

 onMissionTypeChange(): void {
 
  const totalSheetsControl = this.missionForm.get('totalSheets');
  
  totalSheetsControl?.setValidators([Validators.min(0)]); // Allow 0 or positive numbers
  totalSheetsControl?.updateValueAndValidity();
}

  onCountryChange(): void {
    const countryId = this.missionForm.get('countryId')?.value;
    this.missionForm.patchValue({ cityId: '' });
    this.loadCitiesByCountry(countryId);
  }

  onFilterCountryChange(): void {
    const countryId = this.filterForm.get('countryId')?.value;
    this.filterForm.patchValue({ cityId: '' });
    if (countryId) {
      this.loadCitiesByCountry(countryId);
    }
  }

  onFileSelected(event: any): void {
    const files = Array.from(event.target.files) as File[];
    this.selectedFiles = [];
    this.previewImages = [];

    files.forEach(file => {
      if (file.type.startsWith('image/')) {
        this.selectedFiles.push(file);
        
        const reader = new FileReader();
        reader.onload = (e: any) => {
          this.previewImages.push(e.target.result);
        };
        reader.readAsDataURL(file);
      }
    });
  }

  removeImage(index: number): void {
    this.selectedFiles.splice(index, 1);
    this.previewImages.splice(index, 1);
  }

  openAddModal(): void {
    this.missionForm.reset({ 
      isActive: true,
      missionType: '',
      missionAvailability: ''
    });
    this.isEditing = false;
    this.selectedMission = null;
    this.showModal = true;
    this.error = '';
    this.selectedFiles = [];
    this.previewImages = [];
  }

  editMission(mission: any): void {
  this.selectedMission = mission;
  
  // Format dates for input fields
  const formatDate = (dateString: string) => {
    if (!dateString) return '';
    const date = new Date(dateString);
    return date.toISOString().slice(0, 16); // Format for datetime-local input
  };

  // Set form values for editing
  this.missionForm.patchValue({
    missionTitle: mission.missionTitle || '',
    missionDescription: mission.missionDescription || '',
    missionOrganisationName: mission.missionOrganisationName || '',
    missionOrganisationDetail: mission.missionOrganisationDetail || '',
    countryId: mission.countryId || '',
    cityId: mission.cityId || '', 
    startDate: formatDate(mission.startDate),
    endDate: formatDate(mission.endDate),
    missionType: mission.missionType || '',
    totalSheets: mission.totalSheets || '',
    registrationDeadLine: formatDate(mission.registrationDeadLine),
    missionThemeId: mission.missionThemeId || '',
    missionSkillId: mission.missionSkillId || '',
    missionAvailability: mission.missionAvailability || '',
    missionVideoUrl: mission.missionVideoUrl || '',
    isActive: mission.isActive !== undefined ? mission.isActive : true,
    createdAt: mission.createdAt 
  });

  // Load cities for the selected country
  if (mission.countryId) {
    this.loadCitiesByCountry(mission.countryId);
  }

  // Update totalSheets validation based on mission type
  this.onMissionTypeChange();

  this.isEditing = true;
  this.showModal = true;
  this.error = '';
  this.selectedFiles = [];
  this.previewImages = [];
}

async saveMission(): Promise<void> {
  if (this.missionForm.invalid) {
    this.markFormGroupTouched(this.missionForm);
    return;
  }

  // Validate dates
  const startDate = new Date(this.missionForm.get('startDate')?.value);
  const endDate = new Date(this.missionForm.get('endDate')?.value);
  const registrationDeadline = this.missionForm.get('registrationDeadLine')?.value;

  if (startDate >= endDate) {
    this.error = 'Start date must be before end date';
    return;
  }

  if (registrationDeadline && new Date(registrationDeadline) >= startDate) {
    this.error = 'Registration deadline must be before start date';
    return;
  }

  this.loading = true;
  this.error = '';

  try {
    const formValues = this.missionForm.value;

    if (this.isEditing && this.selectedMission) {
      
      if (!formValues.countryId || isNaN(parseInt(formValues.countryId))) {
        this.error = 'Please select a valid country';
        this.loading = false;
        return;
      }
      
      if (!formValues.cityId || isNaN(parseInt(formValues.cityId))) {
        this.error = 'Please select a valid city';
        this.loading = false;
        return;
      }
      
      if (!formValues.missionThemeId || isNaN(parseInt(formValues.missionThemeId))) {
        this.error = 'Please select a valid mission theme';
        this.loading = false;
        return;
      }

      // Fix: Ensure totalSheets is always a valid integer or 0
      let totalSheets = 0;
      if (formValues.totalSheets) {
        totalSheets = parseInt(formValues.totalSheets) || 0;
      }


      const updateMissionData = {
        missionTitle: formValues.missionTitle?.trim() || '',
        missionDescription: formValues.missionDescription?.trim() || '',
        missionOrganisationName: formValues.missionOrganisationName?.trim() || '',
        missionOrganisationDetail: formValues.missionOrganisationDetail?.trim() || '',
        countryId: parseInt(formValues.countryId),
        cityId: parseInt(formValues.cityId),
        startDate: formValues.startDate ? new Date(formValues.startDate).toISOString() : new Date().toISOString(),
        endDate: formValues.endDate ? new Date(formValues.endDate).toISOString() : new Date().toISOString(),
        missionType: formValues.missionType?.trim() || 'TIME',
        totalSheets: totalSheets, // Always include totalSheets
        registrationDeadLine: formValues.registrationDeadLine
          ? new Date(formValues.registrationDeadLine).toISOString()
          : new Date().toISOString(),
        missionThemeId: parseInt(formValues.missionThemeId),
        missionSkillId: formValues.missionSkillId ? parseInt(formValues.missionSkillId) : null,
        missionImages: '',
        missionDocuments: '',
        missionAvailability: formValues.missionAvailability?.trim() || '',
        missionVideoUrl: formValues.missionVideoUrl?.trim() || '',
        isActive: formValues.isActive ?? true
      };
      
      console.log('Sending:', updateMissionData);

      this.http.put(`${this.apiUrl}/mission/${this.selectedMission.id}`, updateMissionData, {
        headers: this.getAuthHeaders()
      }).subscribe({
        next: (response) => {
          console.log('Mission updated successfully:', response);
          this.loadMissions();
          this.closeModal();
          this.loading = false;
        },
        error: (err) => {
          console.error('Full error:', err);
          console.error('Error status:', err.status);
          console.error('Error message:', err.error?.message);
          
          if (err.error?.errors) {
            console.error('Validation errors:', err.error.errors);
            let errorMessages: string[] = [];
            Object.keys(err.error.errors).forEach(key => {
              const errors = err.error.errors[key];
              if (Array.isArray(errors)) {
                errorMessages.push(...errors);
              } else {
                errorMessages.push(errors);
              }
            });
            this.error = `Validation failed: ${errorMessages.join(', ')}`;
          } else if (err.error?.title) {
            this.error = err.error.title;
          } else if (err.error?.message) {
            this.error = err.error.message;
          } else {
            this.error = 'Failed to update mission';
          }
          this.loading = false;
        }
      });
    }else {
      // For creating new mission, use CreateWithImages endpoint with FormData
      const formData = new FormData();
      
      // Add mission data - ensure required fields are present
      if (formValues.missionTitle?.trim()) {
        formData.append('missionTitle', formValues.missionTitle.trim());
      }
      if (formValues.missionDescription?.trim()) {
        formData.append('missionDescription', formValues.missionDescription.trim());
      }
      if (formValues.missionOrganisationName?.trim()) {
        formData.append('missionOrganisationName', formValues.missionOrganisationName.trim());
      }
      if (formValues.missionOrganisationDetail?.trim()) {
        formData.append('missionOrganisationDetail', formValues.missionOrganisationDetail.trim());
      }
      if (formValues.countryId) {
        formData.append('countryId', formValues.countryId.toString());
      }
      if (formValues.cityId) {
        formData.append('cityId', formValues.cityId.toString());
      }
      if (formValues.startDate) {
        formData.append('startDate', formValues.startDate);
      }
      if (formValues.endDate) {
        formData.append('endDate', formValues.endDate);
      }
      if (formValues.missionType?.trim()) {
        formData.append('missionType', formValues.missionType.trim());
      }
      
      if (formValues.totalSheets) {
        formData.append('totalSheets', formValues.totalSheets.toString());
      }
      
      if (formValues.registrationDeadLine) {
        formData.append('registrationDeadLine', formValues.registrationDeadLine);
      }
      
      if (formValues.missionThemeId) {
        formData.append('missionThemeId', formValues.missionThemeId.toString());
      }
      if (formValues.missionSkillId) {
        formData.append('missionSkillId', formValues.missionSkillId.toString());
      }
      
      if (formValues.missionAvailability?.trim()) {
        formData.append('missionAvailability', formValues.missionAvailability.trim());
      }
      
      if (formValues.missionVideoUrl?.trim()) {
        formData.append('missionVideoUrl', formValues.missionVideoUrl.trim());
      }
      
      formData.append('isActive', (formValues.isActive ?? true).toString());

      // Add selected image files
      if (this.selectedFiles && this.selectedFiles.length > 0) {
        this.selectedFiles.forEach((file, index) => {
          formData.append('images', file, file.name);
        });
      }

      this.http.post(`${this.apiUrl}/mission/CreateWithImages`, formData, {
        headers: this.getAuthHeadersForFormData()
      }).subscribe({
        next: (response) => {
          console.log('Mission created successfully:', response);
          this.loadMissions();
          this.closeModal();
          this.loading = false;
        },
        error: (err) => {
          console.error('Error creating mission', err);
          console.error('Full error response:', err);
          console.error('Error status:', err.status);
          
          if (err.error?.errors) {
            console.error('Validation errors:', err.error.errors);
            const validationErrors = Object.values(err.error.errors).flat();
            this.error = `Validation failed: ${validationErrors.join(', ')}`;
          } else {
            this.error = err.error?.message || err.error?.title || err.message || 'Failed to create mission';
          }
          this.loading = false;
        }
      });
    }
  } catch (error) {
    console.error('Error in saveMission:', error);
    this.error = 'Failed to process mission data';
    this.loading = false;
  }
}
   



  deleteMission(id: number): void {
    if (!confirm('Are you sure you want to delete this mission?')) return;

    this.loading = true;
    this.http.delete(`${this.apiUrl}/mission/${id}`, {
      headers: this.getAuthHeaders()
    }).subscribe({
      next: () => {
        this.loadMissions();
        this.loading = false;
      },
      error: err => {
        console.error('Error deleting mission', err);
        this.error = err.error?.message || 'Failed to delete mission';
        this.loading = false;
      }
    });
  }

  applyFilters(): void {
    this.currentPage = 1;
    this.loadMissions();
  }

  clearFilters(): void {
    this.filterForm.reset();
    this.filteredCities = [];
    this.currentPage = 1;
    this.loadMissions();
  }

  closeModal(): void {
    this.showModal = false;
    this.isEditing = false;
    this.selectedMission = null;
    this.missionForm.reset();
    this.error = '';
    this.selectedFiles = [];
    this.previewImages = [];
  }

  previousPage(): void {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.loadMissions();
    }
  }

  nextPage(): void {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
      this.loadMissions();
    }
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages && page !== this.currentPage) {
      this.currentPage = page;
      this.loadMissions();
    }
  }

  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();
    });
  }

  getErrorMessage(fieldName: string): string {
    const control = this.missionForm.get(fieldName);
    if (control?.errors && control.touched) {
      if (control.errors['required']) {
        return `${this.getFieldDisplayName(fieldName)} is required`;
      }
      if (control.errors['maxlength']) {
        return `${this.getFieldDisplayName(fieldName)} cannot exceed ${control.errors['maxlength'].requiredLength} characters`;
      }
      if (control.errors['min']) {
        return `${this.getFieldDisplayName(fieldName)} must be at least ${control.errors['min'].min}`;
      }
    }
    return '';
  }

  private getFieldDisplayName(fieldName: string): string {
    const fieldNames: { [key: string]: string } = {
      'missionTitle': 'Mission Title',
      'missionDescription': 'Mission Description',
      'missionOrganisationName': 'Organisation Name',
      'missionOrganisationDetail': 'Organisation Detail',
      'countryId': 'Country',
      'cityId': 'City',
      'startDate': 'Start Date',
      'endDate': 'End Date',
      'missionType': 'Mission Type',
      'totalSheets': 'Total Sheets',
      'missionThemeId': 'Mission Theme',
      'missionSkillId': 'Mission Skills',
      'missionVideoUrl': 'Video URL'
    };
    return fieldNames[fieldName] || fieldName;
  }

  trackByFn(index: number, item: any): any {
    return item.id || index;
  }

  getPageNumbers(): number[] {
    const pages: number[] = [];
    const start = Math.max(1, this.currentPage - 2);
    const end = Math.min(this.totalPages, this.currentPage + 2);
    
    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    
    return pages;
  }

  formatDate(dateString: string): string {
    if (!dateString) return '-';
    return new Date(dateString).toLocaleDateString();
  }

  getImageUrls(imagesString: string): string[] {
    if (!imagesString) return [];
    return imagesString.split(',').map(path => path.trim()).filter(path => path);
  }

  Math = Math;
}