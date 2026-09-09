import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../Services/Auth Service/auth-service';

export const RoleGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  //role duoc phep di qua
  const expectedRoles = route.data?.['expectedRoles'] as Array<string>;
  if (!expectedRoles || expectedRoles.length === 0) {
    return true; 
  }
  const roleData = authService.getRoleFromToken();
  const userRoles = Array.isArray(roleData) ? roleData : [roleData];
  const hasPermission = expectedRoles.some(role => userRoles.includes(role));
  if (!hasPermission) {
    router.navigate(['/waiting-permission']);
    return false;
  }
  return true;
};
