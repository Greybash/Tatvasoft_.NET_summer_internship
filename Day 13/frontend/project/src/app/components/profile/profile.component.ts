import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { UserProfileService } from '../../services/userprofile.service';
import { CommonModule } from '@angular/common';
import { NavbarComponent } from '../navbar/navbar.component';
import { 
  UserDetail, 
  Country, 
  City, 
  CreateUserDetailRequest, 
  UpdateUserDetailRequest 
} from '../../models/user-profile.model';
import { UserNavComponent } from '../usernav/usernav.component';


@Component({
  selector: 'app-user-profile',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule,],
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss']
})
export class UserProfileComponent implements OnInit {
getCurrentDateTime() {
throw new Error('Method not implemented.');
}
  userProfileForm!: FormGroup;
  userDetail: UserDetail | null = null;
  countries: Country[] = [];
  cities: City[] = [];
  filteredCities: City[] = [];
  
  isLoading = false;
  isEditing = false;
  showCreateForm = false;
  selectedFiles: File[] = [];
  profileImageUrl: string = '';
  error: string = '';
  
  availabilityOptions = [
    { value: 'Full-time', label: 'Full-time' },
    { value: 'Part-time', label: 'Part-time' },
    { value: 'Weekends', label: 'Weekends' },
    { value: 'Evenings', label: 'Evenings' },
    { value: 'Flexible', label: 'Flexible' }
  ];

  skills: string[] = [];
  currentSkill = '';

  constructor(
    private fb: FormBuilder,
    private userProfileService: UserProfileService
  ) {
    this.initializeForm();
  }

  ngOnInit(): void {
    this.loadCountries();
    this.loadUserProfile();
  }

  private initializeForm(): void {
    this.userProfileForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      surname: ['', [Validators.required, Validators.maxLength(100)]],
      employeeId: ['', [Validators.required, Validators.maxLength(50)]],
      manager: ['', [Validators.maxLength(100)]],
      title: ['', [Validators.maxLength(100)]],
      department: ['', [Validators.maxLength(100)]],
      myProfile: ['', [Validators.maxLength(1000)]],
      whyIVolunteer: ['', [Validators.maxLength(1000)]],
      countryId: [null, [Validators.required]],
      cityId: [null, [Validators.required]],
      availability: ['', [Validators.maxLength(100)]],
      linkedInUrl: ['', [Validators.pattern(/^https?:\/\/(www\.)?linkedin\.com\/.*$/)]],
      status: [true]
    });
  }

  private loadUserProfile(): void {
    this.isLoading = true;
    this.error = '';
    
    this.userProfileService.getMyProfile().subscribe({
      next: (profile: UserDetail) => {
        this.userDetail = profile;
        this.populateForm(profile);
        if (profile.countryId) {
          this.loadCitiesForCountry(profile.countryId);
        }
        this.setProfileImage(profile.userImage);
        this.parseSkills(profile.mySkills);
        this.isLoading = false;
      },
      error: (error: any) => {
        this.isLoading = false;
        if (error.status === 404) {
          // User profile not found, show create form
          this.showCreateForm = true;
        } else {
          console.error('Error loading profile:', error);
          this.error = this.getErrorMessage(error);
        }
      }
    });
  }

  private loadCountries(): void {
    this.userProfileService.getCountries().subscribe({
      next: (countries: Country[]) => {
        this.countries = countries;
      },
      error: (error: any) => {
        console.error('Error loading countries:', error);
        this.error = 'Failed to load countries';
      }
    });
  }

  private loadCitiesForCountry(countryId: number): void {
    if (!countryId) {
      this.filteredCities = [];
      return;
    }
    
    this.userProfileService.getCitiesByCountry(countryId).subscribe({
      next: (cities: City[]) => {
        this.filteredCities = cities;
      },
      error: (error: any) => {
        console.error('Error loading cities:', error);
        this.error = 'Failed to load cities';
      }
    });
  }

  onCountryChange(): void {
    const countryId = this.userProfileForm.get('countryId')?.value;
    this.userProfileForm.patchValue({ cityId: null });
    this.loadCitiesForCountry(countryId);
  }

  private populateForm(profile: UserDetail): void {
    this.userProfileForm.patchValue({
      name: profile.name || '',
      surname: profile.surname || '',
      employeeId: profile.employeeId || '',
      manager: profile.manager || '',
      title: profile.title || '',
      department: profile.department || '',
      myProfile: profile.myProfile || '',
      whyIVolunteer: profile.whyIVolunteer || '',
      countryId: profile.countryId || null,
      cityId: profile.cityId || null,
      availability: profile.availability || '',
      linkedInUrl: profile.linkedInUrl || '',
      status: profile.status !== undefined ? profile.status : true
    });
  }

  private setProfileImage(imagePath: string): void {
    if (this.userDetail?.userImage) {
      this.profileImageUrl = this.userDetail.userImage;
      return;
    }
    
    // Fallback to default image
    this.profileImageUrl = '/assets/images/default-mission.jpg';
  }

  private parseSkills(skillsString: string): void {
    if (skillsString) {
      this.skills = skillsString.split(',').map(skill => skill.trim()).filter(skill => skill);
    } else {
      this.skills = [];
    }
  }

  onFileSelected(event: any): void {
    const files = event.target.files;
    if (files && files.length > 0) {
      this.selectedFiles = Array.from(files);
      
      // Preview the first selected image
      const file = files[0];
      if (file.type.startsWith('image/')) {
        const reader = new FileReader();
        reader.onload = (e: any) => {
          this.profileImageUrl = e.target.result;
        };
        reader.readAsDataURL(file);
      }
    }
  }

  addSkill(): void {
    const skillToAdd = this.currentSkill.trim();
    if (skillToAdd && !this.skills.includes(skillToAdd)) {
      this.skills.push(skillToAdd);
      this.currentSkill = '';
    }
  }

  removeSkill(skill: string): void {
    this.skills = this.skills.filter(s => s !== skill);
  }

  private getSkillsString(): string {
    return this.skills.join(', ');
  }

  onSubmit(): void {
    if (this.userProfileForm.invalid) {
      this.markFormGroupTouched();
      return;
    }

    // Validate required fields
    const formValues = this.userProfileForm.value;
    if (!formValues.countryId || !formValues.cityId) {
      this.error = 'Please select both country and city';
      return;
    }

    this.error = '';
    const formData = {
      ...formValues,
      mySkills: this.getSkillsString(),
      countryId: parseInt(formValues.countryId),
      cityId: parseInt(formValues.cityId)
    };

    if (this.showCreateForm) {
      this.createUserProfile(formData);
    } else {
      this.updateUserProfile(formData);
    }
  }

  private createUserProfile(formData: CreateUserDetailRequest): void {
    this.isLoading = true;
    this.error = '';
    
    if (this.selectedFiles.length > 0) {
      this.userProfileService.createUserDetailWithImage(formData, this.selectedFiles).subscribe({
        next: (response: any) => {
          this.isLoading = false;
          this.showSuccessMessage(response.message || 'Profile created successfully');
          this.showCreateForm = false;
          this.resetForm();
          this.loadUserProfile();
        },
        error: (error: any) => {
          this.isLoading = false;
          this.error = this.getErrorMessage(error);
        }
      });
    } else {
      this.userProfileService.createUserDetail(formData).subscribe({
        next: (response: any) => {
          this.isLoading = false;
          this.showSuccessMessage(response.message || 'Profile created successfully');
          this.showCreateForm = false;
          this.resetForm();
          this.loadUserProfile();
        },
        error: (error: any) => {
          this.isLoading = false;
          this.error = this.getErrorMessage(error);
        }
      });
    }
  }

  private updateUserProfile(formData: UpdateUserDetailRequest): void {
    this.isLoading = true;
    this.error = '';
    
    if (this.selectedFiles.length > 0) {
      this.userProfileService.updateUserDetailWithImage(formData, this.selectedFiles).subscribe({
        next: (response: any) => {
          this.isLoading = false;
          this.showSuccessMessage(response.message || 'Profile updated successfully');
          this.isEditing = false;
          this.selectedFiles = [];
          this.loadUserProfile();
        },
        error: (error: any) => {
          this.isLoading = false;
          this.error = this.getErrorMessage(error);
        }
      });
    } else {
      this.userProfileService.updateUserDetail(formData).subscribe({
        next: (response: any) => {
          this.isLoading = false;
          this.showSuccessMessage(response.message || 'Profile updated successfully');
          this.isEditing = false;
          this.loadUserProfile();
        },
        error: (error: any) => {
          this.isLoading = false;
          this.error = this.getErrorMessage(error);
        }
      });
    }
  }

  editProfile(): void {
    this.isEditing = true;
    this.error = '';
  }

  cancelEdit(): void {
    this.isEditing = false;
    this.selectedFiles = [];
    this.error = '';
    if (this.userDetail) {
      this.populateForm(this.userDetail);
      this.setProfileImage(this.userDetail.userImage);
      this.parseSkills(this.userDetail.mySkills);
    }
  }

  cancelCreate(): void {
    this.showCreateForm = false;
    this.resetForm();
  }

  private resetForm(): void {
    this.userProfileForm.reset({
      status: true
    });
    this.skills = [];
    this.currentSkill = '';
    this.selectedFiles = [];
    this.profileImageUrl = '';
    this.error = '';
  }

  deleteProfile(): void {
    if (!confirm('Are you sure you want to delete your profile? This action cannot be undone.')) {
      return;
    }

    this.isLoading = true;
    this.error = '';
    
    this.userProfileService.deleteUserDetail().subscribe({
      next: (response: any) => {
        this.isLoading = false;
        this.showSuccessMessage(response.message || 'Profile deleted successfully');
        this.userDetail = null;
        this.showCreateForm = true;
        this.resetForm();
      },
      error: (error: any) => {
        this.isLoading = false;
        this.error = this.getErrorMessage(error);
      }
    });
  }

  private markFormGroupTouched(): void {
    Object.keys(this.userProfileForm.controls).forEach(key => {
      this.userProfileForm.get(key)?.markAsTouched();
    });
  }

  private getErrorMessage(error: any): string {
    if (error.error?.errors) {
      // Handle validation errors
      const validationErrors = Object.values(error.error.errors).flat();
      return `Validation failed: ${validationErrors.join(', ')}`;
    } else if (error.error?.message) {
      return error.error.message;
    } else if (error.error?.title) {
      return error.error.title;
    } else if (error.message) {
      return error.message;
    } else {
      return 'An unexpected error occurred';
    }
  }

  private showSuccessMessage(message: string): void {
    // Replace with your preferred notification system
    alert(message);
    console.log('Success:', message);
  }

  // Helper methods for template
  isFieldInvalid(fieldName: string): boolean {
    const field = this.userProfileForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  getFieldError(fieldName: string): string {
    const field = this.userProfileForm.get(fieldName);
    if (field && field.errors && (field.dirty || field.touched)) {
      if (field.errors['required']) {
        return `${this.getFieldDisplayName(fieldName)} is required`;
      }
      if (field.errors['maxlength']) {
        return `${this.getFieldDisplayName(fieldName)} cannot exceed ${field.errors['maxlength'].requiredLength} characters`;
      }
      if (field.errors['pattern']) {
        return `${this.getFieldDisplayName(fieldName)} format is invalid`;
      }
    }
    return '';
  }

  private getFieldDisplayName(fieldName: string): string {
    const fieldNames: { [key: string]: string } = {
      'name': 'Name',
      'surname': 'Surname',
      'employeeId': 'Employee ID',
      'manager': 'Manager',
      'title': 'Title',
      'department': 'Department',
      'myProfile': 'Profile Description',
      'whyIVolunteer': 'Why I Volunteer',
      'countryId': 'Country',
      'cityId': 'City',
      'availability': 'Availability',
      'linkedInUrl': 'LinkedIn URL'
    };
    return fieldNames[fieldName] || fieldName;
  }

  // Track by function for ngFor performance
  trackByFn(index: number, item: any): any {
    return item.id || index;
  }
}