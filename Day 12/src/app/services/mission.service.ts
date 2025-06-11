import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Mission {
  images: any;
  imageUrl: any;
  id: number;
  missionTitle: string;
  missionDescription: string;
  missionOrganisationName: string;
  missionOrganisationDetail?: string;
  countryId: number;
  cityId: number;
  missionThemeId: number;
  startDate: string;
  endDate?: string;
  missionType: string;
  totalSheets?: number;
  registrationDeadLine?: string;
  missionSkillId?: number;
  missionImages?: string;
  missionDocuments?: string;
  missionAvailability?: string;
  missionVideoUrl?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
  // Navigation properties
  country?: { id: number; name: string };
  city?: { id: number; name: string };
  missionTheme?: { id: number; name: string };
}

export interface MissionResponse {
  data: Mission[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface MissionFilters {
  title?: string;
  organisationName?: string;
  countryId?: number;
  cityId?: number;
  themeId?: number;
  missionType?: string;
  startDateFrom?: string;
  startDateTo?: string;
  isActive?: boolean;
  sortBy?: string;
  sortDescending?: boolean;
  page?: number;
  pageSize?: number;
}

@Injectable({
  providedIn: 'root'
})
export class MissionService {
  private apiUrl = 'https://localhost:7089/api/Mission'; // Replace with your actual API URL

  constructor(private http: HttpClient) { }

  /**
   * Get all missions with filtering and pagination
   */
  getMissions(filters?: MissionFilters): Observable<MissionResponse> {
    let params = new HttpParams();
    
    if (filters) {
      Object.keys(filters).forEach(key => {
        const value = (filters as any)[key];
        if (value !== null && value !== undefined && value !== '') {
          params = params.set(key, value.toString());
        }
      });
    }

    return this.http.get<MissionResponse>(this.apiUrl, { params });
  }

  /**
   * Get mission by ID
   */
  getMissionById(id: number): Observable<Mission> {
    return this.http.get<Mission>(`${this.apiUrl}/${id}`);
  }

  /**
   * Get all active missions
   */
  getActiveMissions(): Observable<Mission[]> {
    return this.http.get<Mission[]>(`${this.apiUrl}/active`);
  }

  /**
   * Get upcoming missions
   */
  getUpcomingMissions(): Observable<Mission[]> {
    return this.http.get<Mission[]>(`${this.apiUrl}/upcoming`);
  }

  /**
   * Get missions by theme
   */
  getMissionsByTheme(themeId: number): Observable<Mission[]> {
    return this.http.get<Mission[]>(`${this.apiUrl}/theme/${themeId}`);
  }

  /**
   * Get missions by location
   */
  getMissionsByLocation(countryId: number, cityId?: number): Observable<Mission[]> {
    let params = new HttpParams();
    if (cityId) {
      params = params.set('cityId', cityId.toString());
    }
    
    return this.http.get<Mission[]>(`${this.apiUrl}/location/${countryId}`, { params });
  }

  /**
   * Parse comma-separated image paths
   */
  getMissionImages(mission: Mission): string[] {
    if (!mission.missionImages) return [];
    return mission.missionImages.split(',').map(img => img.trim());
  }

  /**
   * Get the first image or a default placeholder
   */
  getFirstImage(mission: Mission): string {
    const images = this.getMissionImages(mission);
    return images.length > 0 ? images[0] : 'assets/images/default-mission.jpg';
  }

  /**
   * Format date for display
   */
  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString();
  }

  /**
   * Check if mission registration is still open
   */
  isRegistrationOpen(mission: Mission): boolean {
    if (!mission.registrationDeadLine) return true;
    return new Date(mission.registrationDeadLine) > new Date();
  }

  /**
   * Get mission status based on dates
   */
  getMissionStatus(mission: Mission): 'upcoming' | 'ongoing' | 'completed' {
    const now = new Date();
    const startDate = new Date(mission.startDate);
    const endDate = mission.endDate ? new Date(mission.endDate) : null;

    if (startDate > now) {
      return 'upcoming';
    } else if (!endDate || endDate > now) {
      return 'ongoing';
    } else {
      return 'completed';
    }
  }
}