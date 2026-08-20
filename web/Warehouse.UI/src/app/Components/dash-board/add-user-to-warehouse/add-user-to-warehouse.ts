import { ChangeDetectorRef, Component, EventEmitter, inject, Input, OnInit, Output } from '@angular/core';
import { WarehouseDTO, WarehouseService } from '../../../Services/warehouse-service';
import { UserService, UsersToWarehouseDTO } from '../../../Services/user-servive';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

export interface UserOption {
  id: string;
  code: string;
  name: string;
  roles?: string[]; 
  isSelected?: boolean;
}

@Component({
  selector: 'app-add-user-to-warehouse',
  imports: [CommonModule, FormsModule],
  templateUrl: './add-user-to-warehouse.html',
  styleUrl: './add-user-to-warehouse.css',
})
export class AddUserToWarehouse implements OnInit {
  @Input() warehouse: WarehouseDTO | null = null;

  @Output() close = new EventEmitter<void>();
  @Output() save = new EventEmitter<{ warehouseId: string, users: UsersToWarehouseDTO[] }>();

  private readonly warehouseService = inject(WarehouseService);
  private readonly userService = inject(UserService);
  private readonly cdr = inject(ChangeDetectorRef);

  keyword: string = '';
  isLoading: boolean = false;
  private loadingTimeout: any;

  users: UserOption[] = [];
  currentPage: number = 1;    
  pageSize: number = 5;
  totalItems: number = 0;
  totalPages: number = 0;
  
  selectedUserId: Set<string> = new Set<string>();

  ngOnInit(): void {
    this.loadUser();
    this.loadExist();
  }

  loadUser() {
    if (this.loadingTimeout) {
      clearTimeout(this.loadingTimeout);
    }
    this.loadingTimeout = setTimeout(() => {
      this.isLoading = true;
      this.cdr.detectChanges();
    }, 250);

    this.userService.getValidUser({
      pageNumber: this.currentPage, 
      pageSize: this.pageSize,
      keyword: this.keyword
    })
    .subscribe({
      next: (res: any) => {
        clearTimeout(this.loadingTimeout);
        this.isLoading = false;
        this.users = res.items || res.Items || [];
        this.totalItems = res?.totalCount || res?.TotalCount || 0;
        this.totalPages = Math.ceil(this.totalItems / this.pageSize) || 1;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Lỗi khi tải danh sách nhân viên', err);
        clearTimeout(this.loadingTimeout);
        this.isLoading = false;
        this.cdr.detectChanges(); 
      }
    });
  }

  loadExist() {
    if (!this.warehouse || !this.warehouse.id) return;
    
    this.userService.getWarehouseUsers(this.warehouse.id).subscribe({
      next: (userIds: string[]) => {
        this.selectedUserId.clear(); 
        
        if (userIds && userIds.length > 0) {
          userIds.forEach(id => this.selectedUserId.add(id));
        }
        
        this.cdr.detectChanges(); 
      },
      error: (err) => {
         console.error('Lỗi khi load danh sách user của kho', err);
         this.cdr.detectChanges();
      }
    });
  }

  search() {
    this.currentPage = 1;
    this.loadUser();
  }

  showSelection(userId: string, event: Event) {
    const isChecked = (event.target as HTMLInputElement).checked; 
    if (isChecked) {
      this.selectedUserId.add(userId); 
    } 
    else {
      this.selectedUserId.delete(userId); 
    }
  }

  isWarehouseSelected(userId: string): boolean {
    return this.selectedUserId.has(userId);
  }

  closeModal() {
    this.close.emit();
  }

  savePermissions() {
    const users: UsersToWarehouseDTO[] = Array.from(this.selectedUserId).map(id => ({
      userId: id
    }));
    
    if (this.warehouse && this.warehouse.id) {
      this.save.emit({
        warehouseId: this.warehouse.id, 
        users: users
      });
    }
    this.closeModal();
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
}