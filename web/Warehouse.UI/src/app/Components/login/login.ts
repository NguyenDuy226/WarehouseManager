import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { AuthService } from '../../Services/auth-service';
import { Router, RouterLink } from '@angular/router';
import { jwtDecode } from 'jwt-decode';

@Component({
  selector: 'app-login',
  imports: [RouterLink, ReactiveFormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  errorMessage: string = '';
  showPassword = false;
  loginForm!: FormGroup;

  private formBuilder = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  ngOnInit() {
    this.loginForm = this.formBuilder.group({
      Email: ['', [Validators.required, Validators.email]],
      Password: ['', [Validators.required, Validators.minLength(6)]],
    });
  }

  seePassword() {
    this.showPassword = !this.showPassword;
  }

  summit() {
    this.errorMessage = '';

    if (this.loginForm.valid) {
      this.authService.login(this.loginForm.value).subscribe({
        next: (response) => {
          localStorage.setItem('token', response.token);
          if (response.refreshToken) {
            localStorage.setItem('refreshToken', response.refreshToken);
          }
          const decodedToken: any = jwtDecode(response.token);
          const role =
            decodedToken.role ||
            decodedToken[
              'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'
            ];

          console.log('Role:', role);
          if (role === 'Admin' || role === 'WAREHOUSE_MANAGER' || role === 'SYSTEM_ADMIN') {
            this.router.navigate(['/manager-dashboard/page/1']);
          } 
          else if (role === 'USER') {
            this.router.navigate(['/waiting-permission']);
          } 
          else {
            this.router.navigate(['/dashboard']);
          }
        },

        error: (err) => {
          console.error(err);

          if (err.status == 400) {
            this.errorMessage = 'Vui lòng nhập đầy đủ thông tin';
          } 
          else if (err.status == 404) {
            this.errorMessage = 'Email không tồn tại';
          } 
          else if (err.status == 401) {
            this.errorMessage = 'Mật khẩu không chính xác';
          } 
          else if (err.status == 429) {
            this.errorMessage = 'Tài khoản đã bị tạm khóa do đăng nhập sai quá nhiều lần, vui lòng thử lại sau 5 phút';
          }
          else if(err.status == 403){
            this.errorMessage = 'Tài khoản của bạn đã bị vô hiệu hóa'
          } 
          else {
            this.errorMessage =
              'Đã có lỗi xảy ra, vui lòng thử lại';
          }

          alert(this.errorMessage);
        }
      });
    } 
    else {
      this.loginForm.markAllAsTouched();
    }
  }
  
}