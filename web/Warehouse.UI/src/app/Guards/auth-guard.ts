import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';

export const AuthGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const token = localStorage.getItem('token');
  if (token == null) {
    router.navigate(['/login']);
    return false;
  }
  try {
    const payload = JSON.parse(atob(token.split('.')[1])); 
    const expired = payload.exp * 1000 < Date.now(); 
    if (expired) {
      localStorage.removeItem('token');
      router.navigate(['/login']);
      return false;
    }
    return true;
  } 
  catch (error) {
    localStorage.removeItem('token');
    router.navigate(['/login']);
    return false;
  }
};