import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { environment } from '../../environments/environment';

export interface User {
  id: string;
  email: string;
  fullName?: string;
  isEmailVerified: boolean;
  lastLoginAt?: Date;
  createdAt: Date;
}

export interface LoginRequest {
  email: string;
}

export interface LoginResponse {
  success: boolean;
  message: string;
  requiresOtp: boolean;
  token?: string;
  user?: User;
}

export interface SignupRequest {
  email: string;
  fullName: string;
}

export interface SignupResponse {
  success: boolean;
  message: string;
  requiresOtp: boolean;
  user?: User;
}

export interface VerifyOtpRequest {
  email: string;
  otpCode: string;
  purpose: string; // 'LOGIN' or 'SIGNUP'
}

export interface VerifyOtpResponse {
  success: boolean;
  message: string;
  token?: string;
  user?: User;
}

export interface ResendOtpRequest {
  email: string;
  purpose: string;
}

export interface ResendOtpResponse {
  success: boolean;
  message: string;
}

export interface AuthResponse {
  success: boolean;
  message: string;
  token?: string;
  user?: User;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private currentUserSubject: BehaviorSubject<User | null>;
  public currentUser: Observable<User | null>;
  private apiUrl = environment.apiUrl + '/auth'; // http://localhost:5000/api/auth

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

  public get isAuthenticated(): boolean {
    const user = this.currentUserValue;
    const token = this.getToken();
    return !!(user && token && !this.isTokenExpired(token));
  }

  public get token(): string | null {
    return this.getToken();
  }

  // Login with email (sends OTP)
  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, request)
      .pipe(
        catchError(this.handleError<LoginResponse>('login'))
      );
  }

  // Signup with email and name (sends OTP)
  signup(request: SignupRequest): Observable<SignupResponse> {
    return this.http.post<SignupResponse>(`${this.apiUrl}/signup`, request)
      .pipe(
        catchError(this.handleError<SignupResponse>('signup'))
      );
  }

  // Verify OTP and get JWT token
  verifyOtp(request: VerifyOtpRequest): Observable<VerifyOtpResponse> {
    return this.http.post<VerifyOtpResponse>(`${this.apiUrl}/verify-otp`, request)
      .pipe(
        map(response => {
          if (response.success && response.token && response.user) {
            this.setAuthData(response.token, response.user);
          }
          return response;
        }),
        catchError(this.handleError<VerifyOtpResponse>('verifyOtp'))
      );
  }

  // Resend OTP
  resendOtp(request: ResendOtpRequest): Observable<ResendOtpResponse> {
    return this.http.post<ResendOtpResponse>(`${this.apiUrl}/resend-otp`, request)
      .pipe(
        catchError(this.handleError<ResendOtpResponse>('resendOtp'))
      );
  }

  // Refresh JWT token
  refreshToken(): Observable<AuthResponse> {
    const headers = this.getAuthHeaders();
    return this.http.post<AuthResponse>(`${this.apiUrl}/refresh-token`, {}, { headers })
      .pipe(
        map(response => {
          if (response.success && response.token && response.user) {
            this.setAuthData(response.token, response.user);
          }
          return response;
        }),
        catchError(this.handleError<AuthResponse>('refreshToken'))
      );
  }

  // Get user profile
  getProfile(): Observable<User> {
    const headers = this.getAuthHeaders();
    return this.http.get<User>(`${this.apiUrl}/profile`, { headers })
      .pipe(
        map(user => {
          this.currentUserSubject.next(user);
          localStorage.setItem('currentUser', JSON.stringify(user));
          return user;
        }),
        catchError(this.handleError<User>('getProfile'))
      );
  }

  // Logout
  logout(): Observable<any> {
    const headers = this.getAuthHeaders();
    return this.http.post(`${this.apiUrl}/logout`, {}, { headers })
      .pipe(
        map(() => {
          this.clearAuthData();
          return { success: true };
        }),
        catchError(() => {
          // Even if logout fails on server, clear local data
          this.clearAuthData();
          return throwError(() => new Error('Logout failed'));
        })
      );
  }

  // Logout locally (without server call)
  logoutLocal(): void {
    this.clearAuthData();
  }

  // Private helper methods
  private setAuthData(token: string, user: User): void {
    localStorage.setItem('authToken', token);
    localStorage.setItem('currentUser', JSON.stringify(user));
    this.currentUserSubject.next(user);
  }

  private clearAuthData(): void {
    localStorage.removeItem('authToken');
    localStorage.removeItem('currentUser');
    this.currentUserSubject.next(null);
  }

  private getToken(): string | null {
    return localStorage.getItem('authToken');
  }

  private getAuthHeaders(): HttpHeaders {
    const token = this.getToken();
    return new HttpHeaders({
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    });
  }

  private isTokenExpired(token: string): boolean {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const exp = payload.exp * 1000; // Convert to milliseconds
      return Date.now() >= exp;
    } catch {
      return true; // If we can't parse, consider it expired
    }
  }

  private handleError<T>(operation = 'operation') {
    return (error: any): Observable<T> => {
      console.error(`${operation} failed:`, error);
      
      // If token is invalid/expired, logout
      if (error.status === 401) {
        this.logoutLocal();
      }
      
      // Extract error message
      let errorMessage = 'An error occurred';
      if (error.error?.message) {
        errorMessage = error.error.message;
      } else if (error.message) {
        errorMessage = error.message;
      }
      
      return throwError(() => new Error(errorMessage));
    };
  }

  // Utility methods for validation
  isValidEmail(email: string): boolean {
    const emailPattern = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
    return emailPattern.test(email);
  }

  formatEmail(email: string): string {
    return email.toLowerCase().trim();
  }
}
