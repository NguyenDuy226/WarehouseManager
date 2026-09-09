import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, EventEmitter, Input, OnInit, Output, inject, HostListener } from '@angular/core';
import { FormsModule } from '@angular/forms';
import * as XLSX from 'xlsx';
import { switchMap } from 'rxjs'; 
import { CreateMaterialDTO, MaterialService } from '../../../Services/Base Entity Service/material-service';
import { SupplierService, SupplierDto } from '../../../Services/Base Entity Service/supplier-service';
import { CategoryService, MaterialCategoryDto } from '../../../Services/Base Entity Service/category-service';
import { UnitOfMeasureDto, UnitOfMeasureService } from '../../../Services/Base Entity Service/unit-of-measure-service';
import { NewMaterial } from './new-material/new-material';
import { NewSupplier } from './new-supplier/new-supplier';
import { DocumentService } from '../../../Services/Document Service/document-service';

export interface MaterialDetail {
  id: string;
  code: string;
  name: string;
  refPrice: number; 
  categoryName: string;
  unitName: string;
}

export interface ReceiptLine {
  materialId: string;
  materialCode: string;
  materialName: string;
  unitName: string;
  categoryName: string;
  quantity: number;
  unitPrice: number;    
  totalAmount: number;
  
  showDropdown?: boolean;
  searchQuery?: string;
}

@Component({
  selector: 'app-receipt',
  standalone: true,
  imports: [CommonModule, FormsModule, NewMaterial, NewSupplier],
  templateUrl: './receipt.html',
  styleUrl: './receipt.css',
})
export class Receipt implements OnInit {
  @Input() warehouseId!: string;
  @Input() warehouseName: string = '';


  @Output() close = new EventEmitter<void>();
  @Output() save = new EventEmitter<void>(); 

  private readonly materialService = inject(MaterialService);
  private readonly supplierService = inject(SupplierService);
  private readonly categoryService = inject(CategoryService);
  private readonly unitService = inject(UnitOfMeasureService);
  // private readonly receiptService = inject(ReceiptService);
  private readonly documentService = inject(DocumentService);
  private readonly cdr = inject(ChangeDetectorRef);

  //header infor
  receiptHeader = {
    warehouseId: '',
    supplierId: '',
    externalDocumentNo: '', 
    documentDate: new Date().toISOString().substring(0, 10),
    reason: '', 
    note: ''
  };

  lines: ReceiptLine[] = [];
  materials: MaterialDetail[] = []; 
  suppliers: SupplierDto[] = [];
  categories: MaterialCategoryDto[] = [];
  units: UnitOfMeasureDto[] = [];

  //temp bool
  isCreatingMaterial = false;
  isCreatingSupplier = false;
  showSupplierDropdown = false;
  supplierSearchQuery = '';
  
  //state
  isSubmitting = false;
  reasonFocused = false;
  noteFocused = false;

  activeNewLine: ReceiptLine | null = null;
  newMaterialData: CreateMaterialDTO = {
    name: '',
    categoryId: '',
    unitOfMeasureId: '',
    refPrice: 0,
    mininumStock: 0
  };

  @HostListener('document:click', ['$event'])
  onClickOutside(event: Event) {
    this.showSupplierDropdown = false;
    this.lines.forEach(l => l.showDropdown = false);
  }

  ngOnInit(): void {
    if (this.warehouseId) {
      this.receiptHeader.warehouseId = this.warehouseId;
    }
    this.loadAllData();
    this.addEmptyLine();
  }

  private loadAllData(): void {
    const pagingReq = { pageNumber: 1, pageSize: 1000 };
    //get materials    
    this.materialService.getAll(pagingReq).subscribe({
      next: (response) => {
        this.materials = response.items.map(m => ({
          id: m.id,
          code: m.code,
          name: m.name,
          unitName: m.unitName,
          categoryName: m.categoryName, 
          refPrice: m.refPrice
        }));
      },
      error: (err) => console.error('Materials load error', err)
    });

    //get suppliers
    this.supplierService.getAll(pagingReq).subscribe({
      next: (res) => this.suppliers = res.items,
      error: (err) => console.error('Supplier load error', err)
    });
    
    //get category
    this.categoryService.getAll(pagingReq).subscribe({
      next: (res) => this.categories = res.items,
      error: (err) => console.error('Category error:', err)
    });

    //get unit
    this.unitService.getAll(pagingReq).subscribe({
      next: (res) => this.units = res.items,
      error: (err) => console.error('Unit error', err)
    });
  }

  // material drop down
  toggleDropdown(line: ReceiptLine, event?: Event): void {
    if (event) event.stopPropagation();
    //close all other dropdown and clear search bar
    this.lines.forEach(l => { 
      if (l !== line) l.showDropdown = false; 
    });
    this.showSupplierDropdown = false; 

    line.showDropdown = !line.showDropdown;
    if (line.showDropdown) {
      line.searchQuery = ''; 
    }
  }

  searchMaterial(query?: string): MaterialDetail[] {
    if (!query || query.trim() === '') return this.materials;
    const lowerQuery = query.toLowerCase().trim();
    return this.materials.filter(m => m.code.toLowerCase().includes(lowerQuery) || 
                                      m.name.toLowerCase().includes(lowerQuery));
  }

  selectMaterial(line: ReceiptLine, mat: MaterialDetail): void {
    line.materialId = mat.id;
    line.materialCode = mat.code;
    line.materialName = mat.name;
    line.unitName = mat.unitName;
    line.categoryName = mat.categoryName;
    line.unitPrice = mat.refPrice;
    
    this.calTotalLine(line);
    line.showDropdown = false; 
  }

  //supplier drop down
  toggleSupplierDropdown(event: Event): void {
    event.stopPropagation(); 
    this.showSupplierDropdown = !this.showSupplierDropdown;
    if (this.showSupplierDropdown) {
      this.supplierSearchQuery = '';
      this.lines.forEach(l => l.showDropdown = false); 
    }
  }

  searchSupplier(query?: string): SupplierDto[] {
    if (!query || query.trim() === '') return this.suppliers;
    const lowerQuery = query.toLowerCase().trim();
    return this.suppliers.filter(s => 
      s.code.toLowerCase().includes(lowerQuery) || 
      s.name.toLowerCase().includes(lowerQuery)
    );
  }

  selectSupplier(sup: SupplierDto): void {
    this.receiptHeader.supplierId = sup.id;
    this.showSupplierDropdown = false;
  }

  get selectedSupplierName(): string {
    const sup = this.suppliers.find(s => s.id === this.receiptHeader.supplierId);
    return sup ? `[${sup.code}] - ${sup.name}` : '';
  }

  //child component
  openCreateMaterial(line: ReceiptLine, event?: Event): void {
    if (event) event.stopPropagation();
    line.showDropdown = false;
    this.activeNewLine = line; 
    this.newMaterialData = {
      name: '',
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
        //gan in current line
        if (this.activeNewLine) {
          this.selectMaterial(this.activeNewLine, newMat);
        }
        //else gan in new line 
        else {
          const newLine: ReceiptLine = {
            materialId: newMat.id,
            materialCode: newMat.code,
            materialName: newMat.name,
            unitName: newMat.unitName, 
            categoryName: newMat.categoryName,
            quantity: 1, 
            unitPrice: newMat.refPrice,
            totalAmount: newMat.refPrice, 
            showDropdown: false,
            searchQuery: ''
          };
          this.lines.push(newLine);
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

  openCreateSupplier(event?: Event){
     if (event) event.stopPropagation(); 
     this.showSupplierDropdown = false; 
     this.isCreatingSupplier = true;
  }

  closeCreateSupplier(){
      this.isCreatingSupplier = false;
  }

  handleCreateSupplier(newSupplier: any){
    if(newSupplier) {
      this.suppliers.unshift(newSupplier);
      this.receiptHeader.supplierId = newSupplier.id;
      this.closeCreateSupplier();
    } 
    else {
      this.loadAllData(); 
    }
  }
  clearSupplier(event: Event): void {
    event.stopPropagation(); 
    this.receiptHeader.supplierId = ''; 
    this.showSupplierDropdown = false; 
  }
  
  //document
  private buildPayload(): any {
    const utcDateString = new Date(this.receiptHeader.documentDate).toISOString();
    return {
      warehouseId: this.receiptHeader.warehouseId || this.warehouseId,
      supplierId: this.receiptHeader.supplierId || null,
      externalDocumentNo: this.receiptHeader.externalDocumentNo.trim(), 
      documentDate: utcDateString,      
      reason: this.receiptHeader.reason.trim(),
      note: this.receiptHeader.note.trim(),
      type: 1,
      lines: this.lines.map(l => ({
        materialId: l.materialId,
        quantity: Number(l.quantity),
        unitPrice: Number(l.unitPrice)
      }))
    };
  }
  saveDraft(): void {
    if (!this.checkValidReceipt()) return;
    const payload = this.buildPayload();
    this.isSubmitting = true;
    this.documentService.createReceipt(payload).subscribe({
      next: (docId) => {
        this.isSubmitting = false;
        alert('Đã lưu nháp phiếu nhập kho thành công!');
        this.save.emit(); 
        this.onClose();
      },
      error: (err) => {
        this.isSubmitting = false;
        console.error(err);
        alert(err.error?.message || 'Có lỗi xảy ra khi lưu nháp phiếu nhập kho.');
      }
    });
  }
  submitForApproval(): void {
    if (!this.checkValidReceipt()) return;
    
    if (confirm('Bạn có chắc chắn muốn gửi phiếu này cho Quản lý phê duyệt? Sau khi gửi sẽ không thể chỉnh sửa!')) {
      const payload = this.buildPayload();
      this.isSubmitting = true;
      this.documentService.createReceipt(payload).pipe(
        switchMap((response: any) => {
          const docId = response?.id || response?.data || response?.value || response;
          return this.documentService.submit(docId);
        })
      )
      .subscribe({
        next: () => {
          this.isSubmitting = false;
          alert('Đã gửi phiếu nhập kho thành công!');
          this.save.emit(); 
          this.onClose();
        },
        error: (err) => {
          this.isSubmitting = false;
          console.error('Lỗi khi gọi API:', err);
          alert(err.message || 'Có lỗi xảy ra khi gửi phê duyệt phiếu nhập kho.');
        }
      });
    }
  }
  private checkValidReceipt(): boolean {
    const warehouseId = this.receiptHeader.warehouseId || this.warehouseId;
    if (!warehouseId) {
      alert('Vui lòng chọn kho nhận hàng!');
      return false;
    }
    if (!this.receiptHeader.documentDate) {
      alert('Vui lòng chọn ngày lập phiếu!');
      return false;
    }
    if (!this.lines || this.lines.length === 0) {
      alert('Phiếu nhập phải có ít nhất 1 dòng vật tư!');
      return false;
    }
    for (let i = 0; i < this.lines.length; i++) {
      const line = this.lines[i];
      const stt = i + 1;
      if (!line.materialId) {
        alert(`Dòng ${stt}: Vui lòng chọn vật tư!`);
        return false;
      }
      if (!line.quantity || Number(line.quantity) <= 0) {
        alert(`Dòng ${stt}: Số lượng phải lớn hơn 0!`);
        return false;
      }
      if (line.unitPrice === null || Number(line.unitPrice) < 0) {
        alert(`Dòng ${stt}: Đơn giá không được âm!`);
        return false;
      }
    }
    const materialIds = this.lines.map(l => l.materialId);
    const hasDuplicate = new Set(materialIds).size !== materialIds.length;
    if (hasDuplicate) {
      alert('Có vật tư bị chọn lặp lại nhiều lần! Vui lòng gộp số lượng trên 1 dòng.');
      return false;
    }
    return true;
  }
  onClose(): void {
    this.close.emit();
  }

  //action
  addEmptyLine(): void {
    this.lines.push({ 
      materialId: '', 
      materialCode: '', 
      materialName: '', 
      unitName: '', 
      categoryName:'',
      quantity: 1, 
      unitPrice: 0, 
      totalAmount: 0,
      showDropdown: false,
      searchQuery: '' 
    });
  }
  removeLine(index: number): void {
    this.lines.splice(index, 1);
  }
  calTotalLine(line: ReceiptLine): void {
    if (line.quantity > 0 && line.unitPrice >= 0) {
      line.totalAmount = Number((line.quantity * line.unitPrice).toFixed(4));
    } 
    else {
      line.totalAmount = 0;
    }
  }
  get grandTotal(): number {
    return this.lines.reduce((sum, line) => sum + (line.totalAmount || 0), 0);
  }
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
      const matchedMaterial = this.materials.find(
        m => m.code.toLowerCase() === rowCode.toLowerCase()
      );

      if (matchedMaterial) {
        const emptyLineIndex = this.lines.findIndex(l => !l.materialId);
        const newLine: ReceiptLine = {
          materialId: matchedMaterial.id,
          materialCode: matchedMaterial.code,
          materialName: matchedMaterial.name,
          unitName: matchedMaterial.unitName,
          categoryName: matchedMaterial.categoryName,
          quantity: rowQty,
          unitPrice: rowPrice > 0 ? rowPrice : matchedMaterial.refPrice,
          totalAmount: 0,
          showDropdown: false
        };
        this.calTotalLine(newLine);

        if (emptyLineIndex !== -1) {
          this.lines[emptyLineIndex] = newLine;
        } 
        else {
          this.lines.push(newLine);
        }
        addedCount++;
      } 
      else {
        notFoundCount++;
      }
    });

    if (notFoundCount > 0) {
      alert(`Nhập thành công ${addedCount} mã. Có ${notFoundCount} mã vật tư bị bỏ qua (Không tìm thấy trong hệ thống).`);
    }
  }
  


}