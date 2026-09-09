import { Component, inject, OnInit } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../Services/Auth Service/auth-service';

@Component({
  selector: 'app-change-password',
  imports: [RouterLink, ReactiveFormsModule, CommonModule],
  templateUrl: './change-password.html',
  styleUrl: './change-password.css',
})
export class ChangePassword implements OnInit {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  changePasswordForm!: FormGroup;
  showOldPassword = false;
  showNewPassword = false;
  showConfirmPassword = false;
  errorMessage: string | null = null;
  successMessage: string | null = null;
  isLoading = false;

  ngOnInit(): void {
    this.changePasswordForm = this.fb.group({
      OldPassword: ['', [Validators.required]],
      NewPassword: ['', [Validators.required, Validators.minLength(6)]],
      ConfirmPassword: ['', [Validators.required]]
    }, {
      validators: this.passwordMatchValidator 
    });
  }

  passwordMatchValidator(control: AbstractControl) {
    const newPassword = control.get('NewPassword')?.value;
    const confirmPassword = control.get('ConfirmPassword')?.value;

    if (newPassword !== confirmPassword) {
      control.get('ConfirmPassword')?.setErrors({ mismatch: true });
      return { mismatch: true };
    } else {
      return null;
    }
  }

  submit() {
    if (this.changePasswordForm.invalid) {
      this.changePasswordForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = null;
    this.successMessage = null;
    const request = {
      oldPassword: this.changePasswordForm.value.OldPassword,
      newPassword: this.changePasswordForm.value.NewPassword
    };
    this.authService.changePassword(request).subscribe({
      next: (res) => {
        this.isLoading = false;
        this.successMessage = 'Đổi mật khẩu thành công!';
          setTimeout(() => {
          this.authService.logout().subscribe(); 
        }, 1500);
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.error?.message || 'Đã có lỗi xảy ra. Vui lòng thử lại sau.';
      }
    });
  }
}