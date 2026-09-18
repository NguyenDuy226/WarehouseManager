import { ChangeDetectorRef, Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { UserDTO, UserService } from '../../Services/Auth Service/user-servive';
import { AuthService } from '../../Services/Auth Service/auth-service';

@Component({
  selector: 'app-user-profile',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './profile.html',
})
export class Profile implements OnInit {
  private readonly userService = inject(UserService);
  private readonly authService = inject(AuthService);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly route = inject(ActivatedRoute);
  isLoading: boolean = false;
  user: UserDTO | null = null;

  ngOnInit(): void {
    const userId = this.authService.getUserIdFromToken();
    if (userId) {
      this.loadProfile(userId);
    } 
    else {
      this.isLoading = false;
    }
  }

  getRoleDisplayName(roles?: string[]): string {
    if (!roles || roles.length === 0 || roles.includes('USER')) return 'Nhân viên'; 
    if (roles.includes('SYSTEM_ADMIN')) return 'Quản trị hệ thống';
    if (roles.includes('WAREHOUSE_MANAGER')) return 'Quản lý kho';
    if (roles.includes('WAREHOUSE_CLERK')) return 'Nhân viên kho';
    if (roles.includes('APPROVER')) return 'Người phê duyệt';
    if (roles.includes('REQUESTER')) return 'Người yêu cầu';
    if (roles.includes('AUDITOR')) return 'Kiểm toán viên';
    return roles.join(', ');
  }

  getInitials(name: string): string {
    if (!name) return 'U';
    return name.charAt(0).toUpperCase();
  }
  loadProfile(userId : string){
    this.isLoading = true;
    this.userService.getById(userId).subscribe({
      next: (res) =>{
        this.user = res;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) =>{
        console.error('Lỗi khi tải thông tin!', err);
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    })
  }


}