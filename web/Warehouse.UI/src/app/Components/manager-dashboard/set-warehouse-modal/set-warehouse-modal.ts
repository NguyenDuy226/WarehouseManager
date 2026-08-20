import { ChangeDetectorRef, Component, EventEmitter, inject, input, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserDTO, UserService, WarehouseRequestDTO } from '../../../Services/user-servive';
import { WarehouseService } from '../../../Services/warehouse-service';
import { PagingRequestWarehouse } from '../../../Services/warehouse-service';

export interface WarehouseOption {
  id: string;
  code: string;
  name: string;
  address: string;
  isSelected: boolean;
}

@Component({
  selector: 'app-set-warehouse-modal',
  imports: [CommonModule, FormsModule],
  templateUrl: './set-warehouse-modal.html'
})
export class SetWarehouseModalComponent implements OnInit {
  @Input() user: UserDTO | null = null;
  
  @Output() close = new EventEmitter<void>();
  @Output() save = new EventEmitter<{ userId: string, warehouses: WarehouseRequestDTO[] }>();

  private readonly warehouseService = inject(WarehouseService);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly userService = inject(UserService);
  selectedWarehouseIds: Set<string> = new Set<string>();

  keyword: string = '';
  isLoading: boolean = false;
  private loadingTimeout: any;

  warehouses: WarehouseOption[] = [];
  currentPage: number = 1;
  pageSize: number = 5;
  totalItems: number = 0;
  totalPages: number = 0;

  ngOnInit(): void {
    this.loadWarehouses();
    this.loadExist();
  }

  loadExist() {
    if (!this.user || !this.user.id) return;
    this.userService.getUserWarehouses(this.user.id).subscribe({
      next: (warehouseIds: string[]) => {
        warehouseIds.forEach(id => this.selectedWarehouseIds.add(id));
        this.cdr.detectChanges(); 
      },
      error: (err) => console.error('Lỗi tải danh sách kho của user', err)
    });
  }

  loadWarehouses() {
    if (this.loadingTimeout) {
      clearTimeout(this.loadingTimeout);
    }
    this.loadingTimeout = setTimeout(() => {
      this.isLoading = true;
      this.cdr.detectChanges();
    }, 250);
    this.warehouseService.getall({
      pageNumber: this.currentPage, 
      pageSize: this.pageSize,
      keyword: this.keyword
    })
    .subscribe({
      next: (response: any) => {
        clearTimeout(this.loadingTimeout);
        this.isLoading = false;
        this.warehouses = response?.items || response?.Items || []; 
        this.totalItems = response?.totalCount || response?.TotalCount || 0;
        this.totalPages = Math.ceil(this.totalItems / this.pageSize) || 1;
        this.cdr.detectChanges(); 
      },
      error: (err) => {
        console.error('Lỗi khi tải kho:', err);
        clearTimeout(this.loadingTimeout);
        this.isLoading = false;
        this.cdr.detectChanges(); 
      }
    });
  }

  onSearch() {
    this.currentPage = 1;
    this.loadWarehouses();
  }
  goToPage(page: number) {
    if (page >= 1 && page <= this.totalPages && page !== this.currentPage) {
      this.currentPage = page;
      this.loadWarehouses();
    }
  }
  toggleSelection(warehouseId: string, event: Event) {
    const isChecked = (event.target as HTMLInputElement).checked; 
    if (isChecked) {
      this.selectedWarehouseIds.add(warehouseId); 
    } else {
      this.selectedWarehouseIds.delete(warehouseId); 
    }
  }
  isWarehouseSelected(warehouseId: string): boolean {
    return this.selectedWarehouseIds.has(warehouseId);
  }
  closeModal() {
    this.close.emit();
  }
  savePermissions() {
    const warehouses: WarehouseRequestDTO[] = Array.from(this.selectedWarehouseIds).map(id => ({
      warehouseId: id
    }));
    if (this.user && this.user.id) {
      this.save.emit({
        userId: this.user.id, 
        warehouses: warehouses
      });
    }
    this.closeModal();
  }
  
}