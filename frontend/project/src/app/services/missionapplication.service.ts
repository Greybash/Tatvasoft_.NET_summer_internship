import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface MissionApplicationRequest {
  MissionId: number;  // Capital M to match .NET
  Seats: number;      // Capital S to match .NET
}

export interface MissionApplication {
  id: number;
  missionId: number;
  userId: number;
  appliedDate: string;
  seats: number;
  status: boolean;        // Approval status
  applystatus: boolean;   // Application submission status
}

@Injectable({
  providedIn: 'root'
})
export class MissionApplicationService {
  private readonly apiUrl = 'https://localhost:7089/api';

  constructor(private http: HttpClient) { }

  /**
   * Apply for a mission
   */
  applyForMission(userId: number, request: MissionApplicationRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/MissionApplication/apply?userId=${userId}`, request);
  }

  /**
   * Get user's applications
   */
  getMyApplications(): Observable<MissionApplication[]> {
    return this.http.get<MissionApplication[]>(`${this.apiUrl}/MissionApplication/my-applications`);
  }

  /**
   * Cancel application
   */
  cancelApplication(applicationId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/MissionApplication/cancel/${applicationId}`);
  }
}