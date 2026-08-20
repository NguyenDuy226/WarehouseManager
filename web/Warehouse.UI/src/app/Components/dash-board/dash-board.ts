import { ChangeDetectorRef, Component, inject, OnInit } from '@angular/core';
import { PagingRequestWarehouse, WarehouseDTO, WarehouseService } from '../../Services/warehouse-service';
import { combineLatest } from 'rxjs';
import { PagedResult, UserService, UsersToWarehouseDTO } from '../../Services/user-servive'; 
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AuthService } from '../../Services/auth-service';
import { AddUserToWarehouse } from "./add-user-to-warehouse/add-user-to-warehouse";
import { CreateWarehouse } from './create-warehouse/create-warehouse';

@Component({
  selector: 'app-dash-board',
  standalone: true,
  imports: [RouterLink, CommonModule, ReactiveFormsModule, FormsModule, AddUserToWarehouse, CreateWarehouse],
  templateUrl: './dash-board.html',
  styleUrl: './dash-board.css',
})
export class DashBoard implements OnInit {
  private readonly warehouseService = inject(WarehouseService);
  private readonly route = inject(ActivatedRoute);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);
  private readonly userService = inject(UserService);

  //filter
  keyword: string = '';
  sortDirection: 'desc' | 'asc' = 'desc'; 
  sortBy: string = 'createdAt'; 
  
  //paging
  pages: (number | string)[] = [];
  isLoading: boolean = false;

  //child component
  isUserToWarehouseOpen: boolean = false;
  selectedWarehouse: WarehouseDTO | null = null;
  isCreateWarehouseOpen: boolean = false;

  pagedResult: PagedResult<WarehouseDTO> = {
    items: [],
    totalCount: 0,
    pageNumber: 1,
    pageSize: 10,
    totalPage: 1,
    hasPreviousPage: false,
    hasNextPage: false
  };
  
  role: string | string[] | null = null;
  isAdminOrManager: boolean = false;

  ngOnInit(): void {
    this.role = this.authService.getRoleFromToken();
    this.checkRole();

    combineLatest([
      this.route.paramMap,
      this.route.queryParamMap
    ])
    .subscribe(([params, queryParams]) => {
      const pageParam = params.get('page');
      const parsedPage = pageParam ? parseInt(pageParam, 10) : 1;
      this.pagedResult.pageNumber = !isNaN(parsedPage) && parsedPage > 0 ? parsedPage : 1;
      this.keyword = queryParams.get('keyword') || '';
      this.sortBy = queryParams.get('sortBy') || 'createdAt';
      this.sortDirection = (queryParams.get('sortDirection') as 'asc' | 'desc') || 'desc';
      this.loadWarehouses();
    });
  }
  
  loadWarehouses() {
    this.isLoading = true;
    const request: PagingRequestWarehouse = {
      pageNumber: this.pagedResult.pageNumber,
      pageSize: this.pagedResult.pageSize,
      keyword: this.keyword,
      sortBy: this.sortBy,
      sortDirection: this.sortDirection        
    };

    this.warehouseService.getall(request).subscribe({
      next: (res) => {
        this.pagedResult = res;
        this.calculatePage();
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Lỗi khi tải danh sách kho:', err);
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  // PAGING 
  private cleanParams() {
    return {
      keyword: this.keyword ? this.keyword : null,
      sortBy: this.sortBy,
      sortDirection: this.sortDirection
    };
  }

  goToPage(page: number | string): void {
    if (typeof page === 'number' && page >= 1 && page <= this.pagedResult.totalPage) {
      this.router.navigate(['/dashboard/page', page], { queryParams: this.cleanParams() });
    }
  }
  nextPage(): void {
    if (this.pagedResult.hasNextPage) this.goToPage(this.pagedResult.pageNumber + 1);
  }
  previousPage(): void {
    if (this.pagedResult.hasPreviousPage) this.goToPage(this.pagedResult.pageNumber - 1);
  }
  private calculatePage(): void {
    const total = this.pagedResult.totalPage; 
    const current = this.pagedResult.pageNumber;
    const pages: (number | string)[] = [];
    
    if (total <= 7) {
      for (let i = 1; i <= total; i++) pages.push(i);
    } 
    else {
      pages.push(1);
      if (current > 3) pages.push('...');
      const start = Math.max(2, current - 1);
      const end = Math.min(total - 1, current + 1);
      for (let i = start; i <= end; i++) pages.push(i);
      if (current < total - 2) pages.push('...');
      pages.push(total);
    }
    this.pages = pages;
  }

  // sort, search
  sort(column: string): void {
    if (this.sortBy === column) {
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortBy = column;
      this.sortDirection = 'asc';
    }
    this.router.navigate(['/dashboard/page/1'], { queryParams: this.cleanParams() });
  }
  search(): void {
    this.router.navigate(['/dashboard/page/1'], { queryParams: this.cleanParams() });
  }
  
  //action
  checkRole() {
    if (!this.role) {
      this.isAdminOrManager = false;
      return;
    }
    const roles = Array.isArray(this.role) ? this.role : [this.role];
    this.isAdminOrManager = roles.some(r => r.toUpperCase() === 'SYSTEM_ADMIN' || r.toUpperCase() === 'WAREHOUSE_MANAGER');
  }
  deleteWarehouse(id: string): void {
    if (confirm('Bạn có chắc chắn muốn xóa kho này không?')) {
      this.isLoading = true;
      this.warehouseService.delete(id).subscribe({
        next: () => {
          this.loadWarehouses();
        },
        error: (err) => {
          alert('Có lỗi xảy ra khi xóa kho!');
          this.isLoading = false;
          this.cdr.detectChanges();
        }
      });
    }
  }
  lockWarehouse(id: string): void {
    const warehouse = this.pagedResult.items.find(w => w.id === id);
    if (!warehouse) return;
    const actionText = warehouse.status ? 'khóa' : 'mở khóa';

    if (confirm(`Bạn có chắc chắn muốn ${actionText} kho này không?`)) {
      this.isLoading = true;
      if(warehouse.status == 1){
        warehouse.status = 0;
        this.warehouseService.update(id, warehouse).subscribe({
          next: () =>{
            this.loadWarehouses(),
            this.isLoading = false;
          }, 
          error: ()=> {
            alert('Lỗi khi khóa kho'),
            this.isLoading = false;
            this.cdr.detectChanges();
          }
        });
      }
      else{
        warehouse.status = 1;
        this.warehouseService.update(id, warehouse).subscribe({
          next: () =>{
            this.loadWarehouses(),
            this.isLoading = false;
          }, 
          error: ()=> {
            alert('Lỗi khi mở khóa kho'),
            this.isLoading = false;
            this.cdr.detectChanges();
          }
        });
      }
    }
  }

  //create kho
  openCreateWarehouse(): void {
    this.isCreateWarehouseOpen = true;
  }
  closeCreateWarehouse(): void{
    this.isCreateWarehouseOpen = false;
  }
  handleCreateWarehouse(): void {
    this.closeCreateWarehouse(); 
    this.loadWarehouses();
    this.router.navigate(['/dashboard/page/1'], { queryParams: this.cleanParams() });
  }

  //add users to warehouse
  openUserToWarehouse(warehouse: WarehouseDTO){
    this.selectedWarehouse = warehouse;
    this.isUserToWarehouseOpen= true;
  }
  closeWarehouseModal(){
    this.isUserToWarehouseOpen = false;
    this.selectedWarehouse = null;
  }
  handleWarehouse(data: { warehouseId: string, users: UsersToWarehouseDTO[] }) {
    this.userService.setUsersToWarehouse(data.warehouseId, data.users).subscribe({
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