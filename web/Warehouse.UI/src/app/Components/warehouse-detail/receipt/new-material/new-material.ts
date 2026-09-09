import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CreateMaterialDTO } from '../../../../Services/Base Entity Service/material-service';

@Component({
  selector: 'app-new-material',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './new-material.html' 
})
export class NewMaterial {
  @Input() categories: any[] = [];
  @Input() units: any[] = [];
  @Input() materialData!: CreateMaterialDTO;
  @Input() existingMaterials: any[] = []; 
  
  @Output() close = new EventEmitter<void>();
  @Output() save = new EventEmitter<void>(); 

  onClose() {
    this.close.emit();
  }

  onSave() {
    const rawName = this.materialData.name || '';
    const validName = rawName.trim().replace(/\s+/g, ' ');
    if (!validName) {
      alert('Vui lòng nhập tên vật tư!');
      return;
    }
    if (this.materialData.refPrice < 0) {
      alert('Giá tham khảo không được là số âm!');
      return;
    }
    if (this.materialData.mininumStock < 0) {
      alert('Tồn kho tối thiểu không được là số âm!');
      return;
    }
    const isDuplicate = this.existingMaterials.some(
      m => m.name.toLowerCase() === validName.toLowerCase()
    );
    if (isDuplicate) {
      alert(`Vật tư mang tên "${validName}" đã tồn tại trong hệ thống!`);
      return;
    }
    this.materialData.name = validName;
    this.save.emit();
 
  }
}