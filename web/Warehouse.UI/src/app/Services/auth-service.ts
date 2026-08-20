import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient, HttpHandlerFn, HttpRequest } from '@angular/common/http';
import { Router } from '@angular/router';
import { catchError, of, tap } from 'rxjs';

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


  private clear() {
    localStorage.removeItem('token');
    localStorage.removeItem('refreshToken');
    this.router.navigate(['/login']);
  }
}

export function AuthInterceptor(req: HttpRequest<unknown>, next: HttpHandlerFn){
  const Token = inject(AuthService).getToken();
  if(Token != null) {
    const newReq = req.clone({
      setHeaders: {
        Authorization: `Bearer ${Token}`  
      }
    })
    return next(newReq);
  }
  return next(req);



}

