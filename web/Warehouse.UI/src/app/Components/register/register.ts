import { Component, inject, OnInit } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../Services/Auth Service/auth-service';

@Component({
  selector: 'app-register',
  imports: [RouterLink, ReactiveFormsModule, CommonModule],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register implements OnInit {
  errorMessage: string = '';
  showPassword = false;
  showConfirmPassword = false;
  registerForm!: FormGroup;

  private formBuilder = inject(FormBuilder); 
  private authService = inject(AuthService);
  private router = inject(Router);

  get f() { 
    return this.registerForm.controls; 
  }

  // Các hàm helper để check điều kiện mật khẩu phục vụ giao diện HTML
  hasMinLength(): boolean {
    return (this.f['Password'].value || '').length >= 6;
  }
  hasUpperCase(): boolean {
    return /[A-Z]/.test(this.f['Password'].value || '');
  }
  hasLowerCase(): boolean {
    return /[a-z]/.test(this.f['Password'].value || '');
  }
  hasNumeric(): boolean {
    return /[0-9]/.test(this.f['Password'].value || '');
  }
  // Hàm helper kiểm tra ký tự đặc biệt mới
  hasSpecialChar(): boolean {
    return /[!@#$%^&*(),.?":{}|<>_\-+~`[\]\/\\;']/.test(this.f['Password'].value || '');
  }

  ngOnInit() {
    this.registerForm = this.formBuilder.group({
      Name: ['', [Validators.required, Validators.minLength(3)]],
      Email: ['', [Validators.required, Validators.email]],
      Password: ['', [Validators.required, Register.complexPasswordValidator]],
      ConfirmPassword: ['', [Validators.required]]
    },
    {
      validators: Register.passwordMatch
    });
  }

  // Custom Validator cập nhật thêm điều kiện kiểm tra ký tự đặc biệt
  static complexPasswordValidator(control: AbstractControl): ValidationErrors | null {
    const value = control.value || '';
    const hasUpperCase = /[A-Z]/.test(value);
    const hasLowerCase = /[a-z]/.test(value);
    const hasNumeric = /[0-9]/.test(value);
    const hasSpecial = /[!@#$%^&*(),.?":{}|<>_\-+~`[\]\/\\;']/.test(value);
    const hasMinLength = value.length >= 6;

    const valid = hasUpperCase && hasLowerCase && hasNumeric && hasSpecial && hasMinLength;
    return valid ? null : { complexPassword: true };
  }

  static passwordMatch(control: AbstractControl): ValidationErrors | null {
    const password = control.get('Password')?.value;
    const confirmPassword = control.get('ConfirmPassword')?.value;
    if (!password || !confirmPassword) {
      return null;
    }
    return password === confirmPassword ? null : { passwordMismatch: true };
  }

  seePassword() {
    this.showPassword = !this.showPassword;
  }

  seeConfirmPassword() {
    this.showConfirmPassword = !this.showConfirmPassword;
  }

  summit() {
    this.errorMessage = '';
    
    if (this.registerForm.valid) {
      this.authService.register(this.registerForm.value).subscribe({
        next: () => {
          alert("Đăng ký thành công!");
          this.router.navigate(['/login']);
        },
        error: (err) => {
          if (err.status === 400) {
            this.errorMessage = "Vui lòng nhập đầy đủ thông tin hoặc mật khẩu chưa đủ mạnh";
          } else if (err.status === 409) {
            this.errorMessage = "Email đã được sử dụng";
          } else {
            this.errorMessage = "Đã có lỗi xảy ra, vui lòng thử lại";
          }
          alert(this.errorMessage);
        }
      });   
    } else {
      this.registerForm.markAllAsTouched();
      if (this.registerForm.errors?.['passwordMismatch']) {
        alert("Mật khẩu và mật khẩu xác nhận không khớp nhau!");
      }
    }
  }
}