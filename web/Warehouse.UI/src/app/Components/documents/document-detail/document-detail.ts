import { ChangeDetectorRef, Component, EventEmitter, inject, Input, OnInit, Output, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Observable, switchMap, throwError } from 'rxjs';
import { DocumentDTO, DocumentLineDTO, DocumentService, DocumentStatus, DocumentType, UpdateDocumentBaseDTO, UpdateIssueDTO, UpdateReceiptDTO, UpdateTransferDTO } from '../../../Services/Document Service/document-service';
import { AuthService } from '../../../Services/Auth Service/auth-service';
import { SupplierService, SupplierDto } from '../../../Services/Base Entity Service/supplier-service';
import { MaterialService, CreateMaterialDTO } from '../../../Services/Base Entity Service/material-service';
import { CategoryService, MaterialCategoryDto } from '../../../Services/Base Entity Service/category-service';
import { UnitOfMeasureDto, UnitOfMeasureService } from '../../../Services/Base Entity Service/unit-of-measure-service';
import { WarehouseDetailRequest, WarehouseDetailService } from '../../../Services/Base Entity Service/warehouse-detail-service';
import { NewSupplier } from '../../warehouse-detail/receipt/new-supplier/new-supplier';
import { NewMaterial } from '../../warehouse-detail/receipt/new-material/new-material';
import { WarehouseDTO, WarehouseEntityStatus, WarehouseService } from '../../../Services/Base Entity Service/warehouse-service';

export interface MaterialDetail {
  id: string;
  code: string;
  name: string;
  refPrice: number; 
  categoryName: string;
  unitName: string;
  stockQuantity?: number; 
}

export interface DocumentLineUI extends DocumentLineDTO {
  showDropdown?: boolean;
  searchQuery?: string;
}

export interface DocumentDetailUI extends Omit<DocumentDTO, 'lines'> {
  lines: DocumentLineUI[];
}

@Component({
  selector: 'app-document-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, NewSupplier, NewMaterial],
  templateUrl: './document-detail.html',
})
export class DocumentDetail implements OnInit {
  @Input() documentId: string | null = null;

  @Output() close = new EventEmitter<void>();
  @Output() actionCompleted = new EventEmitter<{ id: string, newStatus: DocumentStatus }>();

  private readonly documentService = inject(DocumentService);
  private readonly authService = inject(AuthService);
  private readonly supplierService = inject(SupplierService);
  private readonly materialService = inject(MaterialService);
  private readonly categoryService = inject(CategoryService);
  private readonly unitService = inject(UnitOfMeasureService);
  private readonly detailService = inject(WarehouseDetailService);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly warehouseService = inject(WarehouseService);

  document: DocumentDetailUI | null = null;
  isLoading: boolean = false;
  isEditMode: boolean = false; 

  role: string | string[] | null = null;
  isManagerOrAdmin: boolean = false;
  documentStatus = DocumentStatus;
  documentType = DocumentType;

  suppliers: SupplierDto[] = [];
  materials: MaterialDetail[] = []; 
  categories: MaterialCategoryDto[] = [];
  units: UnitOfMeasureDto[] = [];
  warehouses: any[] = []; 


  showSupplierDropdown: boolean = false;
  supplierSearch: string = '';
  showWarehouseDropdown: boolean = false;
  warehouseSearch: string = ''

  //child component state
  isCreatingSupplier: boolean = false;
  isCreatingMaterial: boolean = false;
  activeNewLine: DocumentLineUI | null = null;


  newMaterialData: CreateMaterialDTO = {
    name: '',
    categoryId: '',
    unitOfMeasureId: '',
    refPrice: 0,
    mininumStock: 10
  };

  @HostListener('document:click', ['$event'])
  onClickOutside(event: Event) {
    this.closeAllDropdown();
  }

  closeAllDropdown(): void {
    this.showSupplierDropdown = false;
    if (this.document?.lines) {
      this.document.lines.forEach(l => l.showDropdown = false);
    }
  }

  ngOnInit(): void {
    this.role = this.authService.getRoleFromToken();
    this.checkRole();

    if (this.documentId) {
      this.loadDocumentDetail(this.documentId);
    }
    this.loadAllData();
  }

  //load
  loadDocumentDetail(id: string): void {
    this.isLoading = true;
    this.documentService.getById(id).subscribe({
      next: (res) => {
        const mappedLines: DocumentLineUI[] = res.lines ? res.lines.map((l: DocumentLineDTO) => ({
          ...l,
          showDropdown: false,
          searchQuery: ''
        })) : [];

        this.document = {
          ...res,
          lines: mappedLines
        };
        this.loadByType();
        this.loadWarehouse();
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error(err);
        this.isLoading = false;
        alert('Không tìm thấy chứng từ hoặc có lỗi xảy ra!');
        this.cdr.detectChanges();
        this.onClose(); 
      }
    });
  }

  loadAllData(): void {
    const pagingReq = { pageNumber: 1, pageSize: 2000 };

    this.supplierService.getAll(pagingReq).subscribe({
      next: (res) => this.suppliers = res.items,
      error: (err) => console.error('supplier load error', err)
    });

    this.categoryService.getAll(pagingReq).subscribe({
      next: (res) => this.categories = res.items,
      error: (err) => console.error('category load error', err)
    });

    this.unitService.getAll(pagingReq).subscribe({
      next: (res) => this.units = res.items,
      error: (err) => console.error('unit load error', err)
    });
  }

  loadByType(): void {
    if (!this.document || !this.document.warehouseId) return;
    if (this.document.type === DocumentType.ISSUE || this.document.type === DocumentType.TRANSFER) {
      const request: WarehouseDetailRequest = {
        warehouseId: this.document.warehouseId,
        pageNumber: 1,
        pageSize: 2000, 
        keyword: ''
      };
      this.detailService.getDetail(request).subscribe({
        next: (response) => {
          this.materials = (response.items || [])
            .map((item: any) => ({
              id: item.materialId, 
              code: item.materialCode,
              name: item.materialName,
              unitName: item.unitOfMeasureName || item.unitName,
              categoryName: item.categoryName,
              refPrice: item.price || item.refPrice || 0,
              stockQuantity: item.quantity 
            }));
          this.cdr.detectChanges();
        },
        error: (err) => console.error('Lỗi khi tải danh sách vật tư trong kho:', err)
      });
    } 
    else {
      this.materialService.getAll({ pageNumber: 1, pageSize: 2000 }).subscribe({
        next: (response) => {
          this.materials = response.items.map(m => ({
            id: m.id,
            code: m.code,
            name: m.name,
            unitName: m.unitName,
            categoryName: m.categoryName, 
            refPrice: m.refPrice
          }));
          this.cdr.detectChanges();
        },
        error: (err) => console.error('material load error', err)
      });
    }
  }
  loadWarehouse(): void {
    const request = { pageNumber: 1, pageSize: 1000 }; 
    this.warehouseService.getall(request).subscribe({
      next: (response) => {
        this.warehouses = (response.items || []).filter((w: any) => w.id !== this.document?.warehouseId && w.status == WarehouseEntityStatus.Active);
      },
      error: (err) => console.error('Lỗi khi tải danh sách kho nhận:', err)
    });
  }

  checkRole(): void {
    if (!this.role) {
      this.isManagerOrAdmin = false;
      return;
    }
    const roles = Array.isArray(this.role) ? this.role : [this.role];
    this.isManagerOrAdmin = roles.some(r => {
      const roleStr = r.toUpperCase();
      return roleStr === 'SYSTEM_ADMIN' || roleStr === 'WAREHOUSE_MANAGER' || roleStr === 'APPROVER';
    });
  }

  //supplier action
  toggleSupplierDropdown(event: Event): void {
    event.stopPropagation();
    if (this.isEditMode) {
      this.showSupplierDropdown = !this.showSupplierDropdown;
      if (this.showSupplierDropdown) {
        this.supplierSearch = ''; 
        if (this.document?.lines) {
          this.document.lines.forEach(l => l.showDropdown = false);
        }
      }
    }
  }
  searchSupplier(query?: string): SupplierDto[] {
    if (!query || query.trim() === '') return this.suppliers;
    const lowerQuery = query.toLowerCase().trim();
    return this.suppliers.filter(s => s.code.toLowerCase().includes(lowerQuery) || s.name.toLowerCase().includes(lowerQuery)
    );
  }
  selectSupplier(sup: SupplierDto): void {
    if (this.document) {
      this.document.supplierId = sup.id;
      this.document.supplierName = sup.name;
    }
    this.showSupplierDropdown = false;
  }
  clearSupplier(event: Event): void {
    event.stopPropagation();
    if (this.document) {
      this.document.supplierId = null;
      this.document.supplierName = '';
    }
    this.showSupplierDropdown = false;
  }

  //warehouse action
  toggleWarehouseDropdown(event: Event): void {
    event.stopPropagation();
    if (this.isEditMode) {
      this.showWarehouseDropdown = !this.showWarehouseDropdown;
      if (this.showWarehouseDropdown) {
        this.warehouseSearch = ''; 
        if (this.document?.lines) {
          this.document.lines.forEach(l => l.showDropdown = false);
        }
      }
    }
  }
  searchWarehouse(query?: string): WarehouseDTO[] {
    if (!query || query.trim() === '') return this.warehouses;
    const lowerQuery = query.toLowerCase().trim();
    return this.warehouses.filter(s => s.code.toLowerCase().includes(lowerQuery) || s.name.toLowerCase().includes(lowerQuery)
    );
  }
  selectWarehouse(sup: WarehouseDTO): void {
    if (this.document) {
      this.document.toWarehouseId = sup.id;
      this.document.toWarehouseName = sup.name;
    }
    this.showWarehouseDropdown = false;
  }
  clearWarehouse(event: Event): void {
    event.stopPropagation();
    if (this.document) {
      this.document.toWarehouseId = null;
      this.document.toWarehouseName = '';
    }
    this.showSupplierDropdown = false;
  }


  //child comp 
  openCreateSupplier(event?: Event): void {
    if (event) event.stopPropagation();
    this.showSupplierDropdown = false;
    this.isCreatingSupplier = true;
  }
  closeCreateSupplier(): void {
    this.isCreatingSupplier = false;
  }
  handleCreateSupplier(newSupplier: any): void {
    if (newSupplier) {
      this.suppliers.unshift(newSupplier);
      this.selectSupplier(newSupplier);
      this.closeCreateSupplier();
    } else {
      this.loadAllData();
    }
  }
  toggleDropdown(line: DocumentLineUI, event?: Event): void {
    if (event) event.stopPropagation();
    if (!this.isEditMode) return;

    this.showSupplierDropdown = false;
    if (this.document?.lines) {
      this.document.lines.forEach(l => { 
        if (l !== line) l.showDropdown = false; 
      });
    }

    line.showDropdown = !line.showDropdown;
    if (line.showDropdown) {
      line.searchQuery = ''; 
    }
  }
  searchMaterial(query?: string): MaterialDetail[] {
    if (!query || query.trim() === '') return this.materials;
    const lowerQuery = query.toLowerCase().trim();
    return this.materials.filter(m => 
      m.code.toLowerCase().includes(lowerQuery) || 
      m.name.toLowerCase().includes(lowerQuery)
    );
  }
  selectMaterial(line: DocumentLineUI, mat: MaterialDetail): void {
    line.materialId = mat.id;
    line.itemCode = mat.code;
    line.itemName = mat.name;
    line.unit = mat.unitName;
    line.unitPrice = mat.refPrice || 0;
    line.showDropdown = false; 
  }
  addEmptyLine(): void {
    if (!this.document) return;
    if (!this.document.lines) this.document.lines = [];

    this.document.lines.push({ 
      id: '', 
      materialId: '', 
      itemCode: '', 
      itemName: '', 
      unit: '', 
      quantity: 1, 
      unitPrice: 0, 
      showDropdown: false, 
      searchQuery: ''      
    } as DocumentLineUI);
  }
  removeLine(index: number): void {
    if (this.document?.lines) {
      this.document.lines.splice(index, 1);
    }
  }

  //create mat
  openCreateMaterial(line: DocumentLineUI, event?: Event): void {
    if (event) event.stopPropagation();
    line.showDropdown = false;
    this.activeNewLine = line; 

    this.newMaterialData = {
      name: line.searchQuery || '',
      categoryId: this.categories.length > 0 ? this.categories[0].id : '',
      unitOfMeasureId: this.units.length > 0 ? this.units[0].id : '',
      refPrice: 0,
      mininumStock: 10
    };
    this.isCreatingMaterial = true;
  }

  closeCreateMaterial(): void {
    this.isCreatingMaterial = false;
    this.activeNewLine = null;
  }

  handleCreateMaterial(): void {
    if (!this.newMaterialData.name.trim()) {
      alert('Vui lòng nhập tên vật tư!');
      return;
    }
    this.materialService.create(this.newMaterialData).subscribe({
      next: (res) => {
        const newMat: MaterialDetail = {
          id: res.id,
          code: res.code,
          name: res.name,
          unitName: res.unitName, 
          categoryName: res.categoryName,
          refPrice: res.refPrice
        };

        this.materials.unshift(newMat);

        if (this.activeNewLine) {
          this.selectMaterial(this.activeNewLine, newMat);
        }

        this.cdr.detectChanges();
        this.closeCreateMaterial();
        alert(`Đã tạo thành công vật tư mới: [${res.code}] ${res.name}`);
      },
      error: (err) => {
        console.error(err);
        alert('Đã xảy ra lỗi khi tạo vật tư.');
      }
    });
  }

  //action
  private getUpdate(doc: DocumentDetailUI): Observable<void> {
    const cleanedLines: DocumentLineDTO[] = doc.lines ? doc.lines.map((l: DocumentLineUI) => ({
      id: l.id || undefined, 
      materialId: l.materialId,
      quantity: Number(l.quantity),
      unitPrice: Number(l.unitPrice || 0),
      itemCode: l.itemCode,
      itemName: l.itemName,
      unit: l.unit
    } as DocumentLineDTO)) : [];

    const baseDto: UpdateDocumentBaseDTO = {
      documentDate: doc.createdAt,
      reason: doc.reason,
      note: doc.note,
      lines: cleanedLines
    };

    switch (doc.type) {
      case DocumentType.RECEIPT:
        const receiptDto: UpdateReceiptDTO = {
          ...baseDto,
          supplierId: doc.supplierId, 
          externalDocumentNo: doc.externalDocumentNo || ''
        };
        return this.documentService.updateReceipt(doc.id, receiptDto);

      case DocumentType.ISSUE:
        const issueDto: UpdateIssueDTO = {
          ...baseDto,
          receiver: doc.receiver || '', 
          externalDocumentNo: doc.externalDocumentNo || ''
        };
        return this.documentService.updateIssue(doc.id, issueDto);

      case DocumentType.TRANSFER:
        const transferDto: UpdateTransferDTO = {
          ...baseDto,
          toWarehouseId: doc.toWarehouseId! 
        };
        return this.documentService.updateTransfer(doc.id, transferDto);

      case DocumentType.ADJUSTMENT:
        return this.documentService.updateAdjustment(doc.id, baseDto);

      case DocumentType.OPENING:
        return this.documentService.updateOpening(doc.id, baseDto);

      default:
        return throwError(() => new Error('Loại phiếu không được hỗ trợ cập nhật.'));
    }
  }
  startEdit(): void {
    this.isEditMode = true;
  }
  cancelEdit(isConfirmedFromClose: boolean = false): void {
    if (isConfirmedFromClose || confirm('Bạn có chắc chắn muốn hủy các thay đổi chưa lưu?')) {
      this.isEditMode = false;
      if (this.documentId) {
        this.loadDocumentDetail(this.documentId);
      }
    }
  }
  saveDraft(): void {
    if (!this.document) return;
    this.isLoading = true;
    this.getUpdate(this.document).subscribe({
      next: () => {
        alert('Đã lưu bản nháp thành công!');
        this.isEditMode = false;
        this.loadDocumentDetail(this.document!.id); 
      },
      error: (err) => {
        alert(err.error?.message || 'Có lỗi xảy ra khi lưu nháp!');
        this.isLoading = false;
      }
    });
  }
  submitDocument(): void {
    if (!this.document) return;
    if (this.document.type === DocumentType.ISSUE || this.document.type === DocumentType.TRANSFER) {
      for (let i = 0; i < this.document.lines.length; i++) {
        const line = this.document.lines[i];
        const mat = this.materials.find(m => m.id === line.materialId);
        if (mat && mat.stockQuantity !== undefined && Number(line.quantity) > mat.stockQuantity) {
           alert(`Dòng ${i + 1}: Không thể xuất ${line.quantity}. Tồn kho hiện tại chỉ còn ${mat.stockQuantity}!`);
           return;
        }
      }
    }

    if (confirm('Sau khi gửi phiếu, bạn sẽ không thể tiếp tục chỉnh sửa. Bạn có chắc chắn gửi?')) {
      this.isLoading = true;
      this.getUpdate(this.document).pipe(switchMap(() => this.documentService.submit(this.document!.id)))
      .subscribe({
        next: () => {
          this.actionCompleted.emit({ id: this.document!.id, newStatus: DocumentStatus.PENDING_APPROVAL });
          this.isEditMode = false;
          this.loadDocumentDetail(this.document!.id);
        },
        error: (err) => {
          alert(err.error?.message || 'Có lỗi xảy ra khi gửi phiếu!');
          this.isLoading = false;
        }
      });
    }
  }
  approveDoc(): void {
    if (!this.document) return;
    if (confirm(`Bạn có chắc chắn muốn DUYỆT phiếu [${this.document.code}] không?`)) {
      this.isLoading = true;
      this.documentService.approve(this.document.id).subscribe({
        next: () => {
          this.actionCompleted.emit({ id: this.document!.id, newStatus: DocumentStatus.POSTED });
          this.loadDocumentDetail(this.document!.id);
        },
        error: (err) => {
          alert(err.error?.message || 'Có lỗi xảy ra khi duyệt phiếu!');
          this.isLoading = false;
        }
      });
    }
  }

  rejectDoc(): void {
    if (!this.document) return;
    const reason = prompt(`Nhập lý do từ chối phiếu [${this.document.code}]:`);
    if (reason === null) return; 
    if (reason.trim() === '') {
      alert('Vui lòng nhập lý do từ chối!');
      return;
    }

    this.isLoading = true;
    this.documentService.reject(this.document.id, reason).subscribe({
      next: () => {
        this.actionCompleted.emit({ id: this.document!.id, newStatus: DocumentStatus.REJECTED });
        this.loadDocumentDetail(this.document!.id);
      },
      error: (err) => {
        alert(err.error?.message || 'Có lỗi xảy ra khi từ chối phiếu!');
        this.isLoading = false;
      }
    });
  }
  cancelDoc(): void {
    if (!this.document) return;
    if (confirm(`Bạn có chắc chắn muốn TỪ CHỐI phiếu [${this.document.code}] không?`)) {
      this.isLoading = true;
      this.documentService.cancel(this.document.id).subscribe({
        next: () => {
          this.actionCompleted.emit({ id: this.document!.id, newStatus: DocumentStatus.CANCELED });
          this.loadDocumentDetail(this.document!.id);
        },
        error: (err) => {
          alert(err.error?.message || 'Có lỗi xảy ra khi từ chối phiếu!');
          this.isLoading = false;
        }
      });
    }
  }
  onClose(): void {
    this.close.emit();
  }
  handleClose(): void {
    if (this.isEditMode) {
      const isConfirmed = confirm("Bạn đang chỉnh sửa phiếu. Các thay đổi sẽ không được lưu, bạn có chắc chắn muốn đóng?");
      if (isConfirmed) {
        this.cancelEdit(true); 
        this.onClose();   
      }
    } else {
      this.onClose();
    }
  }

}