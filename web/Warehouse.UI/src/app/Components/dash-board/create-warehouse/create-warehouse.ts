import { Component, EventEmitter, inject, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { WarehouseService } from '../../../Services/Base Entity Service/warehouse-service';

@Component({
  selector: 'app-create-warehouse',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './create-warehouse.html'
})
export class CreateWarehouse {
  @Output() close = new EventEmitter<void>();
  @Output() saved = new EventEmitter<void>(); 

  private readonly fb = inject(FormBuilder);
  private readonly warehouseService = inject(WarehouseService);

  warehouseForm: FormGroup;
  isSubmitting: boolean = false;

  constructor() {
    this.warehouseForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(255)]],
      address: ['', [Validators.required]],
    });
  }

  isFieldInvalid(field: string): boolean {
    const control = this.warehouseForm.get(field);
    return !!(control && control.invalid && (control.dirty || control.touched));
  }

  closeModal() {
    if (!this.isSubmitting) {
      this.close.emit();
    }
  }

  onSubmit() {
    if (this.warehouseForm.invalid) {
      this.warehouseForm.markAllAsTouched();
      return;
    }
    this.isSubmitting = true;
    const formValue = this.warehouseForm.value;
    this.warehouseService.create(formValue).subscribe({
      next: (res) => {
        this.isSubmitting = false;
        this.saved.emit(); 
        this.closeModal();
      },
      error: (err) => {
        alert('Tạo kho mới không thành công')
        this.isSubmitting = false;
      }
    });
  }
}