

// services/user-profile.service.ts
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { 
  UserDetail, 
  Country, 
  City, 
  CreateUserDetailRequest, 
  UpdateUserDetailRequest, 
  ApiResponse 
} from '../models/user-profile.model';

@Injectable({
  providedIn: 'root'
})
export class UserProfileService {
  private apiUrl = 'https://localhost:7089/api'; // Adjust your API base URL
  
  constructor(private http: HttpClient) {}

  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('token'); // Adjust based on your token storage
    return new HttpHeaders({
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    });
  }

  private getFormHeaders(): HttpHeaders {
    const token = localStorage.getItem('token');
    return new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });
  }

  // User Profile Methods
  getMyProfile(): Observable<UserDetail> {
    return this.http.get<UserDetail>(`${this.apiUrl}/UserDetail/MyProfile`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  getUserProfile(userId: number): Observable<UserDetail> {
    return this.http.get<UserDetail>(`${this.apiUrl}/UserDetail/User/${userId}`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  createUserDetail(request: CreateUserDetailRequest): Observable<ApiResponse<UserDetail>> {
    return this.http.post<ApiResponse<UserDetail>>(`${this.apiUrl}/UserDetail`, request, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  updateUserDetail(request: UpdateUserDetailRequest): Observable<ApiResponse<UserDetail>> {
    return this.http.put<ApiResponse<UserDetail>>(`${this.apiUrl}/UserDetail`, request, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  deleteUserDetail(): Observable<ApiResponse<any>> {
    return this.http.delete<ApiResponse<any>>(`${this.apiUrl}/UserDetail`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  uploadProfileImage(files: File[]): Observable<ApiResponse<any>> {
    const formData = new FormData();
    files.forEach(file => {
      formData.append('files', file);
    });

    return this.http.post<ApiResponse<any>>(`${this.apiUrl}/UserDetail/UploadProfileImage`, formData, {
      headers: this.getFormHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  createUserDetailWithImage(userDetail: CreateUserDetailRequest, profileImages?: File[]): Observable<ApiResponse<UserDetail>> {
    const formData = new FormData();
    
    // Append user detail data
    Object.keys(userDetail).forEach(key => {
      formData.append(key, (userDetail as any)[key]);
    });

    // Append profile images if provided
    if (profileImages && profileImages.length > 0) {
      profileImages.forEach(file => {
        formData.append('profileImages', file);
      });
    }

    return this.http.post<ApiResponse<UserDetail>>(`${this.apiUrl}/UserDetail/CreateWithImage`, formData, {
      headers: this.getFormHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  updateUserDetailWithImage(userDetail: UpdateUserDetailRequest, profileImages?: File[]): Observable<ApiResponse<UserDetail>> {
    const formData = new FormData();
    
    // Append user detail data
    Object.keys(userDetail).forEach(key => {
      formData.append(key, (userDetail as any)[key]);
    });

    // Append profile images if provided
    if (profileImages && profileImages.length > 0) {
      profileImages.forEach(file => {
        formData.append('profileImages', file);
      });
    }

    return this.http.put<ApiResponse<UserDetail>>(`${this.apiUrl}/UserDetail/UpdateWithImage`, formData, {
      headers: this.getFormHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  // Location Methods
  getCountries(): Observable<Country[]> {
    return this.http.get<Country[]>(`${this.apiUrl}/Location/countries`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  getCountry(id: number): Observable<Country> {
    return this.http.get<Country>(`${this.apiUrl}/Location/countries/${id}`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  getCitiesByCountry(countryId: number): Observable<City[]> {
    return this.http.get<City[]>(`${this.apiUrl}/Location/countries/${countryId}/cities`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  getAllCities(): Observable<City[]> {
    return this.http.get<City[]>(`${this.apiUrl}/Location/cities`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  getCity(id: number): Observable<City> {
    return this.http.get<City>(`${this.apiUrl}/Location/cities/${id}`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError)
    );
  }

  private handleError(error: any): Observable<never> {
    console.error('API Error:', error);
    return throwError(() => error);
  }
}