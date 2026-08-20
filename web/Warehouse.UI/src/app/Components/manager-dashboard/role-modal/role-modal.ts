import { Component, EventEmitter, inject, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { UserDTO, UserService } from '../../../Services/user-servive';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-role-modal',
  imports: [CommonModule, FormsModule],
  templateUrl: './role-modal.html',
  styleUrl: './role-modal.css',
})
export class RoleModalComponent implements OnChanges {
  @Input() user: UserDTO | null = null; 
  
  @Output() closeModal = new EventEmitter<void>();
  @Output() roleUpdated = new EventEmitter<void>();

  private readonly userService = inject(UserService);

  selectedRole: string = 'USER';
  isSavingRole: boolean = false;

  availableRoles = [
    { value: 'WAREHOUSE_MANAGER', label: 'Quản lý kho' },
    { value: 'WAREHOUSE_CLERK', label: 'Nhân viên kho' },
    { value: 'APPROVER', label: 'Người phê duyệt' },
    { value: 'REQUESTER', label: 'Người yêu cầu' },
    { value: 'AUDITOR', label: 'Kiểm toán viên' },
    { value: 'USER', label: 'Nhân viên (Cơ bản)' }
  ];

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['user'] && this.user) {
      this.selectedRole = (this.user.roles && this.user.roles.length > 0) ? this.user.roles[0] : 'USER';
    }
  }

  onClose(): void {
    this.closeModal.emit();
  }

  submitRoleChange(): void {
    if (!this.user) return;
    
    this.isSavingRole = true;
    const request = { role: this.selectedRole };

    this.userService.setRole(this.user.id, request).subscribe({
      next: () => {
        this.isSavingRole = false;
        alert(`Đã cập nhật quyền thành công cho tài khoản ${this.user?.userName}!`);
        this.roleUpdated.emit();
        this.onClose(); 
      },
      error: (err) => {
        console.error('Lỗi khi phân quyền:', err);
        this.isSavingRole = false;
        if(err.status === 403){
          alert('Chỉ có quản trị viên mới có thể cấp quyền quản lý kho');
        }
        else alert('Có lỗi xảy ra khi cập nhật quyền. Vui lòng thử lại!');
        this.onClose();
      }
    });
  }
}