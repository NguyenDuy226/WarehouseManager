import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, EventEmitter, Input, OnInit, Output, inject, HostListener } from '@angular/core';
import { FormsModule } from '@angular/forms';
import * as XLSX from 'xlsx';
import { switchMap } from 'rxjs'; 
import { WarehouseDetailRequest, WarehouseDetailService } from '../../../Services/Base Entity Service/warehouse-detail-service';
import { WarehouseEntityStatus, WarehouseService } from '../../../Services/Base Entity Service/warehouse-service'; 
import { DocumentService } from '../../../Services/Document Service/document-service';

export interface Materials {
  materialId: string;
  code: string;
  name: string;
  categoryName: string;
  unitName: string;
  currentStock: number; 
}

export interface TransferLine {
  materialId: string;
  materialCode: string;
  materialName: string;
  unitName: string;
  categoryName: string;
  quantity: number;
  unitPrice: number;    
  totalAmount: number;
  currentStock?: number; 
  
  showDropdown?: boolean;
  searchQuery?: string;
}

@Component({
  selector: 'app-transfer',
  imports: [CommonModule, FormsModule], 
  templateUrl: './transfer.html',
  styleUrl: './transfer.css', 
})
export class TransferComponent implements OnInit {
  @Input() warehouseId!: string; 
  @Input() warehouseName: string = '';

  @Output() close = new EventEmitter<void>();
  @Output() save = new EventEmitter<void>(); 

  private readonly detailService = inject(WarehouseDetailService);
  private readonly documentService = inject(DocumentService);
  private readonly warehouseService = inject(WarehouseService);
  private readonly cdr = inject(ChangeDetectorRef);

  transferHeader = {
    toWarehouseId: '', 
    documentDate: new Date().toISOString().substring(0, 10),
    reason: '', 
    note: ''
  };

  lines: TransferLine[] = [];
  materials: Materials[] = []; 
  targetWarehouses: any[] = []; 

  isSubmitting = false;
  reasonFocused = false;
  noteFocused = false;

  @HostListener('document:click', ['$event'])
  onClickOutside(event: Event) {
    this.lines.forEach(l => l.showDropdown = false);
  }

  ngOnInit(): void {
    if (this.warehouseId) {
      this.loadAllData();
      this.loadWarehouse();
    }
    this.addEmptyLine();
  }

  private loadAllData(): void {
    const request: WarehouseDetailRequest = {
      warehouseId: this.warehouseId,
      pageNumber: 1,
      pageSize: 2000, 
      keyword: ''
    };
    
    this.detailService.getDetail(request).subscribe({
      next: (response) => {
        this.materials = (response.items || [])
          .filter((item: any) => item.quantity > 0) 
          .map((item: any) => ({
            materialId: item.materialId, 
            code: item.materialCode,
            name: item.materialName,
            unitName: item.unitOfMeasureName,
            categoryName: item.categoryName,
            currentStock: item.quantity 
          }));
      },
      error: (err) => console.error('Lỗi khi tải danh sách vật tư trong kho:', err)
    });
  }

  private loadWarehouse(): void {
    const request = { pageNumber: 1, pageSize: 1000 }; 
    this.warehouseService.getall(request).subscribe({
      next: (response) => {
        this.targetWarehouses = (response.items || []).filter((w: any) => w.id !== this.warehouseId && w.status == WarehouseEntityStatus.Active);
      },
      error: (err) => console.error('Lỗi khi tải danh sách kho nhận:', err)
    });
  }

  //drop down
  toggleDropdown(line: TransferLine, event?: Event): void {
    if (event) event.stopPropagation();
    this.lines.forEach(l => { if (l !== line) l.showDropdown = false; });
    line.showDropdown = !line.showDropdown;
    if (line.showDropdown) line.searchQuery = ''; 
  }

  searchMaterial(query?: string): Materials[] {
    if (!query || query.trim() === '') return this.materials;
    const lowerQuery = query.toLowerCase().trim();
    return this.materials.filter(m => m.code.toLowerCase().includes(lowerQuery) || m.name.toLowerCase().includes(lowerQuery));
  }

  selectMaterial(line: TransferLine, mat: Materials): void {
    line.materialId = mat.materialId;
    line.materialCode = mat.code;
    line.materialName = mat.name;
    line.unitName = mat.unitName;
    line.categoryName = mat.categoryName;
    line.currentStock = mat.currentStock;
    line.unitPrice = 0; 
    
    this.calTotalLine(line);
    line.showDropdown = false; 
  }

  //action for button
  private buildPayload(): any {
    const localDate = new Date(this.transferHeader.documentDate + 'T00:00:00');
    const utcDateString = localDate.toISOString();
    
    return {
      warehouseId: this.warehouseId,
      toWarehouseId: this.transferHeader.toWarehouseId, 
      documentDate: utcDateString,      
      reason: this.transferHeader.reason.trim(),
      note: this.transferHeader.note.trim(),
      lines: this.lines.map(l => ({
        materialId: l.materialId,
        quantity: Number(l.quantity),
        unitPrice: Number(l.unitPrice)
      }))
    };
  }
  saveDraft(): void {
    if (!this.checkValidTransfer()) return;
    
    const payload = this.buildPayload();
    this.isSubmitting = true;
    
    this.documentService.createTransfer(payload).subscribe({
      next: () => {
        this.isSubmitting = false;
        alert('Đã lưu nháp phiếu chuyển kho thành công!');
        this.save.emit(); 
        this.onClose();
      },
      error: (err) => {
        this.isSubmitting = false;
        console.error(err);
        alert(err.error?.message || 'Có lỗi xảy ra khi lưu nháp phiếu chuyển kho.');
      }
    });
  }
  submitForApproval(): void {
    if (!this.checkValidTransfer()) return;
    
    if (confirm('Bạn có chắc chắn muốn gửi phiếu chuyển kho này cho Quản lý phê duyệt?')) {
      const payload = this.buildPayload();
      this.isSubmitting = true;
      
      this.documentService.createTransfer(payload).pipe(
        switchMap((response: any) => {
          const docId = response?.id || response?.data || response?.value || response;
          if (!docId) throw new Error('Không lấy được ID phiếu từ hệ thống.');
          return this.documentService.submit(docId);
        })
      )
      .subscribe({
        next: () => {
          this.isSubmitting = false;
          alert('Đã gửi phê duyệt phiếu chuyển kho thành công!');
          this.save.emit(); 
          this.onClose();
        },
        error: (err) => {
          this.isSubmitting = false;
          console.error('Lỗi khi gọi API:', err);
          alert(err.message || err.error?.message || 'Có lỗi xảy ra khi gửi phê duyệt phiếu chuyển kho.');
        }
      });
    }
  }
  private checkValidTransfer(): boolean {
    if (!this.warehouseId) {
      alert('Vui lòng chọn kho xuất hàng!');
      return false;
    }
    if (!this.transferHeader.toWarehouseId) {
      alert('Vui lòng chọn Kho nhận hàng!');
      return false;
    }
    if (this.warehouseId === this.transferHeader.toWarehouseId) {
      alert('Kho xuất và Kho nhận không được trùng nhau!');
      return false;
    }
    if (!this.transferHeader.documentDate) {
      alert('Vui lòng chọn ngày lập phiếu!');
      return false;
    }
    if (!this.lines || this.lines.length === 0) {
      alert('Phiếu chuyển kho phải có ít nhất 1 dòng vật tư!');
      return false;
    }
    
    for (let i = 0; i < this.lines.length; i++) {
      const line = this.lines[i];
      const stt = i + 1;
      
      if (!line.materialId) {
        alert(`Dòng ${stt}: Vui lòng chọn vật tư!`);
        return false;
      }
      
      if (line.quantity === null || line.quantity === undefined || line.quantity.toString() === '' || Number(line.quantity) <= 0) {
        alert(`Dòng ${stt}: Số lượng chuyển phải lớn hơn 0!`);
        return false;
      }

      if (line.currentStock !== undefined && Number(line.quantity) > line.currentStock) {
        alert(`Dòng ${stt}: Không thể chuyển ${line.quantity}. Tồn kho hiện tại chỉ còn ${line.currentStock}!`);
        return false;
      }
      
      if (line.unitPrice === null || line.unitPrice === undefined || line.unitPrice.toString() === '' || Number(line.unitPrice) < 0) {
        alert(`Dòng ${stt}: Đơn giá không được âm!`);
        return false;
      }
    }
    
    const materialIds = this.lines.map(l => l.materialId);
    if (new Set(materialIds).size !== materialIds.length) {
      alert('Có vật tư bị chọn lặp lại nhiều lần! Vui lòng gộp số lượng trên 1 dòng.');
      return false;
    }
    return true;
  }
  onClose(): void {
    this.close.emit();
  }

  addEmptyLine(): void {
    this.lines.push({ 
      materialId: '', materialCode: '', materialName: '', 
      unitName: '', categoryName:'', quantity: 1, unitPrice: 0, 
      totalAmount: 0, showDropdown: false, searchQuery: '' 
    });
  }
  removeLine(index: number): void {
    this.lines.splice(index, 1);
  }
  calTotalLine(line: TransferLine): void {
    if (line.quantity > 0 && line.unitPrice >= 0) {
      line.totalAmount = Number((line.quantity * line.unitPrice).toFixed(4));
    } else {
      line.totalAmount = 0;
    }
  }
  get grandTotal(): number {
    return this.lines.reduce((sum, line) => sum + (line.totalAmount || 0), 0);
  }

  //excel
  triggerExcelUpload(): void {
    document.getElementById('excelFileInput')?.click();
  }
  onFileChange(event: any): void {
    const target: DataTransfer = <DataTransfer>(event.target);
    if (!target.files || target.files.length !== 1) {
      alert('Vui lòng chỉ chọn 1 file duy nhất.');
      return;
    }

    const reader = new FileReader();
    reader.onload = (e: any) => {
      const arrayBuffer = e.target.result;
      const workbook: XLSX.WorkBook = XLSX.read(arrayBuffer, { type: 'array' });
      const sheetName: string = workbook.SheetNames[0];
      const worksheet: XLSX.WorkSheet = workbook.Sheets[sheetName];

      const excelData = XLSX.utils.sheet_to_json(worksheet, { defval: '' });
      this.parseExcelData(excelData);
    };
    reader.readAsArrayBuffer(target.files[0]);
    event.target.value = ''; 
  }

  private parseExcelData(data: any[]): void {
    let addedCount = 0;
    let notFoundCount = 0;

    data.forEach((row: any) => {
      const rowCode = (row['Mã VT'] || '').toString().trim();
      const rowQty = parseFloat(row['Số lượng']) || 0;
      const rowPrice = parseFloat(row['Đơn giá']) || 0; 

      if (!rowCode || rowQty <= 0) return;

      const matchedMaterial = this.materials.find(m => m.code.toLowerCase() === rowCode.toLowerCase());

      if (matchedMaterial) {
        const emptyLineIndex = this.lines.findIndex(l => !l.materialId);
        const newLine: TransferLine = {
          materialId: matchedMaterial.materialId,
          materialCode: matchedMaterial.code,
          materialName: matchedMaterial.name,
          unitName: matchedMaterial.unitName,
          categoryName: matchedMaterial.categoryName,
          currentStock: matchedMaterial.currentStock,
          quantity: rowQty,
          unitPrice: rowPrice,
          totalAmount: 0,
          showDropdown: false
        };
        this.calTotalLine(newLine);

        if (emptyLineIndex !== -1) {
          this.lines[emptyLineIndex] = newLine;
        } else {
          this.lines.push(newLine);
        }
        addedCount++;
      } else {
        notFoundCount++;
      }
    });

    if (notFoundCount > 0) {
      alert(`Nhập thành công ${addedCount} mã. Có ${notFoundCount} mã vật tư bị bỏ qua do KHÔNG CÓ TRONG KHO XUẤT.`);
    }
  }
  
}