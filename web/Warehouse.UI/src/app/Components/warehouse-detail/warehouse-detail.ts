import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms'; 
import { Receipt } from "./receipt/receipt";
import { WarehouseDetailDTO, WarehouseDetailRequest, WarehouseDetailService } from '../../Services/Base Entity Service/warehouse-detail-service';
import { WarehouseDTO, WarehouseEntityStatus, WarehouseService } from '../../Services/Base Entity Service/warehouse-service';
import { Issue } from './issue/issue';
import { TransferComponent } from './transfer/transfer';

@Component({
  selector: 'app-warehouse-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, Receipt, Issue, TransferComponent], 
  templateUrl: './warehouse-detail.html',
  styleUrl: './warehouse-detail.css',
})

export class WarehouseDetail implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly warehouseService = inject(WarehouseService);
  private readonly detailService = inject(WarehouseDetailService); 
  private readonly cdr = inject(ChangeDetectorRef);

  warehouse: WarehouseDTO | null = null;
  warehouseId: string | null = null;
  isLoading = true;
  errorMessage = '';

  details: WarehouseDetailDTO[] = [];
  isLoadingDetail = false;
  inventoryError = '';
  detailRequest: WarehouseDetailRequest = {
    warehouseId: '',
    pageNumber: 1,
    pageSize: 10,
    keyword: ''
  };
  totaCount = 0;
  totalPages = 0;
  Math = Math;
  readonly EntityStatus = WarehouseEntityStatus;

  //child comp state
  isReceiptOpen = false;
  isIssueOpen = false; 
  isTransferOpen = false;

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      this.warehouseId = params.get('id');

      if (!this.warehouseId) {
        this.errorMessage = 'Không tìm thấy mã kho.';
        this.isLoading = false;
        return;
      }
      this.detailRequest.warehouseId = this.warehouseId;
      this.loadWarehouse(this.warehouseId);
      this.loadDetail();
    });
  }

  //thong tin kho
  loadWarehouse(id: string): void {
    this.isLoading = true;
    this.errorMessage = '';
    this.warehouseService.getById(id).subscribe({
      next: (response: WarehouseDTO) => {
        this.warehouse = response;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (error) => {
        this.isLoading = false;
        this.errorMessage = 'Không thể tải thông tin kho.';
        this.cdr.detectChanges();
      }
    });
  }
  
  //thong tin vat tu
  loadDetail(): void {
    this.isLoadingDetail = true;
    this.inventoryError = '';
    this.detailService.getDetail(this.detailRequest).subscribe({
      next: (response) => {
        this.details = response.items || [];
        this.totaCount = response.totalCount || 0;
        this.totalPages = Math.ceil(this.totaCount / this.detailRequest.pageSize);
        this.isLoadingDetail = false;
        this.cdr.detectChanges();
      },
      error: (error) => {
        this.isLoadingDetail = false;
        this.inventoryError = 'Không thể tải danh sách vật tư.';
        this.cdr.detectChanges();
      }
    });
  }

  onSearchInventory(keyword: string): void {
    this.detailRequest.keyword = keyword;
    this.detailRequest.pageNumber = 1;
    this.loadDetail();
  }

  changePage(page: number): void {
    if (page >= 1 && page <= this.totalPages && page !== this.detailRequest.pageNumber) {
      this.detailRequest.pageNumber = page;
      this.loadDetail();
    }
  }

  goBack(): void {
    this.router.navigate(['/dashboard/page/1']);
  }

  get isActive(): boolean {
    return this.warehouse?.status === WarehouseEntityStatus.Active;
  }

  get statusText(): string {
    if (!this.warehouse) { return ''; }
    return this.warehouse.status === WarehouseEntityStatus.Active ? 'Hoạt động' : 'Đã khóa';
  }

  //child component
  openReceipt(): void {
    if (this.isActive) {
      this.isReceiptOpen = true;
    }
  }
  closeReceipt(): void {
    this.isReceiptOpen = false;
  }
  handleReceipt(payload?: any): void {
    this.isReceiptOpen = false;
    this.loadDetail();
  }

  openIssue(): void {
    if (this.isActive) {
      this.isIssueOpen = true;
    }
  }
  closeIssue(): void {
    this.isIssueOpen = false;
  }
  handleIssue(): void {
    this.isIssueOpen = false;
    this.loadDetail();
  }

  openTransfer(): void {
    if (this.isActive) {
      this.isTransferOpen = true;
    }
  }
  closeTransfer(): void {
    this.isTransferOpen = false;
  }
  handleTransfer(): void {
    this.isTransferOpen = false;
    this.loadDetail(); 
  }

}