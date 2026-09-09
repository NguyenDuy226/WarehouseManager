import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpHandlerFn, HttpRequest } from '@angular/common/http';
import { Router } from '@angular/router';
import { catchError, of, switchMap, tap, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface LoginRequest {
  email: string;
  password: string;
}
export interface LoginResponse {
  token: string;
  refreshToken: string;
}
export interface RegisterRequest {
  name: string;
  email: string;
  password: string;
  confirmPassword: string;
}
export interface RefreshRequest {
  token: string;
  refreshToken: string;
}
export interface RefreshResponse {
  token: string;
  refreshToken: string;
}
export interface ChangePasswordRequest {
  oldPassword: string;
  newPassword: string;
}

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private router = inject(Router);
  private readonly apiUrl = environment.apiUrl;

  login(request: LoginRequest) {
    return this.http.post<any>(`${this.apiUrl}/auth/login`, request);    
  }
  register (request: RegisterRequest){
    return this.http.post<any>(`${this.apiUrl}/auth/register`, request);    
  }
  getToken() {
    return localStorage.getItem('token');
  }
  refresh(request: RefreshRequest){
    return this.http.post<any>(`${this.apiUrl}/auth/refresh`, request);    
  }
  isLoggedIn() {
    var token = this.getToken();
    if(token == null) return false;
    else return true;
  }
  logout (){
    return this.http.post(`${this.apiUrl}/auth/logout`, {}).pipe(
      tap(() => {
        this.clear();
      }),
      catchError((err) => {
        this.clear();
        return of(null);
      })
    );  
  }
  changePassword(request: ChangePasswordRequest) {
    return this.http.post<any>(`${this.apiUrl}/auth/change-password`, request);
  }
  getRoleFromToken(): string | string[] | null {
    const token = this.getToken();
    if (!token) return null;
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
       const roleClaim = payload['role'] || payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];    
      return roleClaim || null;
    } 
    catch (error) {
      return null;
    }
  }
  getUserIdFromToken(): string | null {
    const token = this.getToken();
    if (!token) return null;
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const userId = payload['sub'] || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];
      return userId || null;
    } 
    catch (error) {
      return null;
    }
  }
  getUserNameFromToken(): string | null{
    const token = this.getToken();
    if (!token) return null;
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const userName = payload['name'] || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'];
      return userName || null;
    } 
    catch (error) {
      return null;
    }
  }


  private clear() {
    localStorage.removeItem('token');
    localStorage.removeItem('refreshToken');
    this.router.navigate(['/login']);
  }
}

let isRefreshing = false;
export function AuthInterceptor(req: HttpRequest<unknown>, next: HttpHandlerFn) {
  //skip login va refresh
  if (req.url.includes('/refresh') || req.url.includes('/login')) {
    return next(req); 
  }
  const authService = inject(AuthService);
  const token = authService.getToken();
  let authReq = req;
  if (token != null) {
    authReq = req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
  }
  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
          if (isRefreshing) {
          return throwError(() => error);
        }
        const refreshToken = localStorage.getItem('refreshToken'); 
        if (!token || !refreshToken) {
          forceLogout(); 
          return throwError(() => error);
        }
        isRefreshing = true;
        const refreshPayload = { token: token, refreshToken: refreshToken };
        return authService.refresh(refreshPayload).pipe(
          switchMap((res: any) => {
            isRefreshing = false;
            const newToken = res.data?.token || res.token;
            const newRefreshToken = res.data?.refreshToken || res.refreshToken;
            localStorage.setItem('token', newToken);
            localStorage.setItem('refreshToken', newRefreshToken);
            const retryReq = req.clone({ setHeaders: { Authorization: `Bearer ${newToken}` } });
            return next(retryReq);
          }),
          catchError((refreshErr) => {
            isRefreshing = false;             
            forceLogout();
            
            return throwError(() => refreshErr);
          })
        );
      }
      
      return throwError(() => error);
    })
  );
}

function forceLogout() {
  alert('Đã có lỗi xảy ra, vui lòng thử lại')
  localStorage.removeItem('token');
  localStorage.removeItem('refreshToken');
  localStorage.clear();
  window.location.href = '/login'; 
}