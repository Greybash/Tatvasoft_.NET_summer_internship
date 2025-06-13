import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { User, LoginRequest, RegisterRequest } from '../models/user.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'https://localhost:7089'; 
  private currentUserSubject: BehaviorSubject<User | null>;
  public currentUser: Observable<User | null>;

  constructor(private http: HttpClient) {
    const storedUser = localStorage.getItem('currentUser');
    this.currentUserSubject = new BehaviorSubject<User | null>(
      storedUser ? JSON.parse(storedUser) : null
    );
    this.currentUser = this.currentUserSubject.asObservable();
  }

  public get currentUserValue(): User | null {
    return this.currentUserSubject.value;
  }

  login(credentials: LoginRequest): Observable<User> {
    return this.http.post<User>(`${this.apiUrl}/auth/login`, credentials)
      .pipe(map(user => {
        localStorage.setItem('currentUser', JSON.stringify(user));
        this.currentUserSubject.next(user);
        return user;
      }));
  }

  register(userData: RegisterRequest): Observable<User> {
    return this.http.post<User>(`${this.apiUrl}/auth/register`, userData)
      .pipe(map(user => {
        localStorage.setItem('currentUser', JSON.stringify(user));
        this.currentUserSubject.next(user);
        return user;
      }));
  }

  logout() {
    localStorage.removeItem('currentUser');
    this.currentUserSubject.next(null);
  }

  isAdmin(): boolean {
    const user = this.currentUserValue;
    return user?.role?.toLowerCase() === 'admin';
  }

  isAuthenticated(): boolean {
    return !!this.currentUserValue;
  }

  /**
   * Get current user ID
   * @returns The current user's ID as a number, or null if not authenticated
   */
  getCurrentUserId(): number | null {
    const user = this.currentUserValue;
    return typeof user?.id === 'number' ? user.id : null;
  }

  /**
   * Get current user ID as Observable
   * @returns Observable of current user ID
   */
  getCurrentUserIdObservable(): Observable<number | null> {
    return this.currentUser.pipe(
      map(user => (typeof user?.id === 'number' ? user.id : null))
    );
  }

  /**
   * Get current user ID synchronously with fallback to localStorage
   * @returns The current user's ID as a number, or null if not found
   */
  getCurrentUserIdWithFallback(): number | null {
    // First try from current user subject
    const user = this.currentUserValue;
    if (typeof user?.id === 'number') {
      return user.id;
    }

    // Fallback to localStorage
    try {
      const storedUser = localStorage.getItem('currentUser');
      if (storedUser) {
        const parsedUser = JSON.parse(storedUser);
        return parsedUser?.id || null;
      }
    } catch (error) {
      console.error('Error parsing stored user:', error);
    }

    return null;
  }
}