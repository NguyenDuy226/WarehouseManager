import { ChangeDetectorRef, Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router'; 
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { PagedResult, PagingRequest, UserDTO, UserService, WarehouseRequestDTO } from '../../Services/user-servive';
import { RoleModalComponent } from './role-modal/role-modal';
import { combineLatest } from 'rxjs';
import { SetWarehouseModalComponent } from "./set-warehouse-modal/set-warehouse-modal";

@Component({
  selector: 'app-manager-dashboard',
  imports: [ReactiveFormsModule, CommonModule, FormsModule, RoleModalComponent, SetWarehouseModalComponent],
  templateUrl: './manager-dashboard.html',
  styleUrl: './manager-dashboard.css',
})
export class ManagerDashboard implements OnInit {
  private readonly userService = inject(UserService);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  //paging
  pages: (number | string)[] = []; 
  isLoading: boolean = false;   
  pagedResult: PagedResult<UserDTO> = {
    items: [],
    totalCount: 0,
    pageNumber: 1,
    pageSize: 10,
    totalPage: 1,
    hasPreviousPage: false,
    hasNextPage: false
  };

  //filter
  keyword: string = '';
  statusFilter: string = 'all'; 
  roleFilter: string = 'all'; 
  sortDirection: 'desc' | 'asc' = 'desc'; 
  //child component
  isRoleModalOpen: boolean = false;
  selectedUserForRole: UserDTO | null = null;
  isWarehouseModalOpen: boolean = false;
  selectedUserForWarehouse: UserDTO | null = null;
  

  ngOnInit(): void {
    combineLatest([
      this.route.paramMap,
      this.route.queryParamMap
    ])
    .subscribe(([params, queryParams]) => {
      const pageParam = params.get('page');
      const parsedPage = pageParam ? parseInt(pageParam, 10) : 1;
      this.pagedResult.pageNumber = !isNaN(parsedPage) && parsedPage > 0 ? parsedPage : 1;
      this.statusFilter = queryParams.get('status') || 'all';          
      this.roleFilter = queryParams.get('role') || 'all';
      this.keyword = queryParams.get('keyword') || '';
      this.sortDirection = (queryParams.get('sort') as 'asc' | 'desc') || 'desc';
      this.loadUsers();
    });
  }

  loadUsers(): void {
    this.isLoading = true; 
    this.cdr.detectChanges(); 
    const request: PagingRequest = { 
      pageNumber: this.pagedResult.pageNumber,
      pageSize: this.pagedResult.pageSize,
      keyword: this.keyword,
      status: this.statusFilter,
      role: this.roleFilter,
      sortBy: 'createdAt',
      sortDirection: this.sortDirection
    };

    this.userService.getAll(request).subscribe({
      next: (res: any) => {
        const dataItems = res.items || res.Items || (Array.isArray(res) ? res : []);
        const totalCount = res.totalCount || res.TotalCount || dataItems.length;
        const pageSize = res.pageSize || res.PageSize || 10;
        const pageNumber = res.pageNumber || res.PageNumber || 1;

        this.pagedResult = {
          items: dataItems,
          totalCount: totalCount,
          pageNumber: pageNumber,
          pageSize: pageSize,
          totalPage: Math.ceil(totalCount / pageSize),
          hasPreviousPage: pageNumber > 1,
          hasNextPage: pageNumber < Math.ceil(totalCount / pageSize)
        };

        this.calculatePages();
        this.isLoading = false; 
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Lỗi khi tải danh sách:', err);
        this.isLoading = false; 
        this.cdr.detectChanges();
      }
    });
  }
  //filter
  toggleSort(): void {
    this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    this.router.navigate(['/manager-dashboard/page/1'], {
      queryParams: { 
        sort: this.sortDirection === 'asc' ? 'asc' : null 
      }, 
      queryParamsHandling: 'merge'
    });
  }
  search(): void {
    this.router.navigate(['/manager-dashboard/page/1'], {
      queryParams: { 
        keyword: this.keyword ? this.keyword : null 
      }, 
      queryParamsHandling: 'merge'
    });
  }
  statusFilterChange(event: any): void {
    this.statusFilter = event.target.value;
    this.router.navigate(['/manager-dashboard/page/1'], {
      queryParams: { 
        status: this.statusFilter !== 'all' ? this.statusFilter : null 
      }, 
      queryParamsHandling: 'merge' 
    });
  }
  roleFilterChange(event: any): void {
    this.roleFilter = event.target.value;
    this.router.navigate(['/manager-dashboard/page/1'], {
      queryParams: { 
        role: this.roleFilter !== 'all' ? this.roleFilter : null 
      }, 
      queryParamsHandling: 'merge' 
    });
  }
  getRoleDisplayName(roles?: string[]): string {
    if (!roles || roles.length === 0 || roles.includes('USER')) return 'Nhân viên'; 
    if (roles.includes('SYSTEM_ADMIN')) return 'Quản trị hệ thống';
    if (roles.includes('WAREHOUSE_MANAGER')) return 'Quản lý kho';
    if(roles.includes('WAREHOUSE_CLERK')) return 'Nhân viên kho';
    if(roles.includes('APPROVER')) return 'Người phê duyệt';
    if(roles.includes('REQUESTER')) return 'Người yêu cầu';
    if(roles.includes('AUDITOR'))return 'Kiểm toán viên';
    return roles.join(', ');
  }
  goToPage(page: number | string): void {
    if (typeof page === 'number' && page >= 1 && page <= this.pagedResult.totalPage) {
      this.router.navigate(['/manager-dashboard/page', page], {
        queryParamsHandling: 'preserve' 
      });
    }
  }
  nextPage(): void {
    if (this.pagedResult.hasNextPage) {
      this.goToPage(this.pagedResult.pageNumber + 1);
    }
  }
  previousPage(): void {
    if (this.pagedResult.hasPreviousPage) {
      this.goToPage(this.pagedResult.pageNumber - 1);
    }
  }
  private calculatePages(): void {
    const total = this.pagedResult.totalPage;
    const current = this.pagedResult.pageNumber;
    const pages: (number | string)[] = [];
    if (total <= 7) {
      for (let i = 1; i <= total; i++) {
        pages.push(i);
      }
    } 
    else {
      pages.push(1);
      if (current > 3) pages.push('...');
      const start = Math.max(2, current - 1);
      const end = Math.min(total - 1, current + 1);
      for (let i = start; i <= end; i++) {
        pages.push(i);
      }
      if (current < total - 2) pages.push('...');
      pages.push(total);
    }
    this.pages = pages;
  }
  toggleUserStatus(user: UserDTO): void {
    const actionName = user.isActive ? 'khóa' : 'mở khóa';
    if (confirm(`Bạn có chắc chắn muốn ${actionName} tài khoản [${user.userName}]?`)) {
      if (user.isActive) {
        this.userService.delete(user.id).subscribe({
          next: () => this.loadUsers(),
          error: () => alert('Lỗi khi khóa tài khoản!')
        });
      } 
      else {
        const updateReq = { name: user.name, isActive: true };
        this.userService.update(user.id, updateReq).subscribe({
          next: () => this.loadUsers(),
          error: () => alert('Lỗi khi mở khóa tài khoản!')
        });
      }
    }
  }
  //role modal
  openRoleModal(user: UserDTO): void { 
    this.selectedUserForRole = user;
    this.isRoleModalOpen = true; 
  }
  closeRoleModal(): void {
    this.isRoleModalOpen = false;
    this.selectedUserForRole = null;
  }
  handleRole(): void {
    this.loadUsers();
  }

  //set warehouse modal
  openWarehouseModal(user: UserDTO){
    this.selectedUserForWarehouse = user;
    this.isWarehouseModalOpen = true;
  }
  closeWarehouseModal(){
    this.isWarehouseModalOpen = false;
    this.selectedUserForWarehouse = null;
  }
  handleWarehouse(data: { userId: string, warehouses: WarehouseRequestDTO[] }) {
    this.userService.setWarehousesToUser(data.userId, data.warehouses).subscribe({
      next: () => {
        alert('Cấp quyền thành công!');
      },
      error: (err) => {
        if(err.status === 400){
          alert('Không thể cấp quyền truy cập kho cho nhân viên');
        }
        console.error(err);
      }
    });
    
  }

}