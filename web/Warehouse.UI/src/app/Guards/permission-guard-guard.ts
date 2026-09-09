import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { catchError, map, of } from 'rxjs';
import { UserService } from '../Services/Auth Service/user-servive';
import { AuthService } from '../Services/Auth Service/auth-service';

export const PermisstionGuard: CanActivateFn = (route, state) => {
  const userService = inject(UserService);
  const authService = inject(AuthService);
  const roles = authService.getRoleFromToken();
  const isAdmin = typeof roles === 'string' ? roles === 'SYSTEM_ADMIN' : roles?.includes('SYSTEM_ADMIN');
  const router = inject(Router);
  const token = authService.getToken();
  
  if(isAdmin) return true;
  if (!token) {
    router.navigate(['/login']);
    return false;
  }
  const handle = (message: string) => {
    const currentUrl = router.url;
    if (currentUrl === '/') {
      return router.createUrlTree(['/dashboard']); 
    }
    else {
      alert(message);
      return false; 
    }
  };
  const targetWarehouseId = route.paramMap.get('id');
  const currentUserId = authService.getUserIdFromToken();
  if (!targetWarehouseId || !currentUserId) {
    return handle('Không tìm thấy thông tin kho hoặc phiên đăng nhập đã hết hạn!');
  }
  return userService.getWarehouseUsers(targetWarehouseId).pipe(
    map((allowedUsers: string[]) => {
      if (allowedUsers.includes(currentUserId)) {
        return true; 
      }
      return handle('Bạn không có quyền truy cập vào kho này!');
    }),
    catchError((error) => {
      console.log(error);
      return of(handle('Bạn chưa được cấp quyền truy cập kho này, vui lòng thử lại'));
    })
  );


};
