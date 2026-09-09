import { Component, EventEmitter, Input, Output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SupplierService, CreateSupplierDto } from '../../../../Services/Base Entity Service/supplier-service';

@Component({
  selector: 'app-new-supplier',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './new-supplier.html' 
})
export class NewSupplier {
  @Input() existingSuppliers: any[] = []; 

  @Output() close = new EventEmitter<void>();
  @Output() created = new EventEmitter<any>();

  private readonly supplierService = inject(SupplierService);

  supplierData: CreateSupplierDto = {
    name: '', 
    taxCode: '', 
    addres: '', 
    contact: ''
  };
  isSubmitting = false;

  onClose() {
    this.close.emit();
  }

  save() {
    const rawName = this.supplierData.name || '';
    const validName = rawName.trim().replace(/\s+/g, ' ');
    if (!validName) {
      alert('Vui lòng nhập tên nhà cung cấp!');
      return;
    }

    const isDuplicate = this.existingSuppliers.some(
      s => s.name.toLowerCase() === validName.toLowerCase()
    );
    if (isDuplicate) {
      alert(`Nhà cung cấp mang tên "${validName}" đã tồn tại trong hệ thống!`);
      return;
    }
    const contactInput = (this.supplierData.contact || '').trim();
    if (contactInput) {
      if (contactInput.includes('@')) {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(contactInput)) {
          alert('Email liên hệ không đúng định dạng!');
          return;
        }
      } 
      else {
        const hasLetters = /[a-zA-ZÀ-ỹà-ỹ]/.test(contactInput);
        if (hasLetters) {
          alert('Số điện thoại không hợp lệ (không được chứa chữ cái).');
          return;
        }
      }
    }
    this.supplierData.name = validName;
    this.supplierData.contact = contactInput;
    this.isSubmitting = true;
    this.supplierService.create(this.supplierData).subscribe({
      next: (res) => {
        this.isSubmitting = false;
        alert(`Đã tạo thành công Nhà cung cấp: [${res.code}] ${res.name}`);
        this.created.emit(res); 
      },
      error: (err) => {
        this.isSubmitting = false;
        console.error(err);
        alert(err.error?.message || 'Có lỗi xảy ra khi tạo nhà cung cấp.');
      }
    });
  }
  
}