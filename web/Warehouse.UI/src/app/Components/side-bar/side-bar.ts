import { ChangeDetectorRef, Component, inject } from '@angular/core';
import { NavigationEnd, Router, RouterModule, RouterOutlet } from '@angular/router';
import { AuthService } from '../../Services/auth-service';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { filter } from 'rxjs';

@Component({
  selector: 'app-side-bar',
  standalone: true,
  templateUrl: './side-bar.html',
  styleUrl: './side-bar.css',
  imports: [RouterOutlet, ReactiveFormsModule, CommonModule,FormsModule, RouterModule],
})
export class SideBar {
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);
  private readonly cdr = inject(ChangeDetectorRef);

  isOpen : boolean = true;
  isAdminOrManager: boolean = false;
  currentUrl = '';

  ngOnInit(): void {
    this.checkUserRole();
    this.currentUrl = this.router.url;
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
      )
      .subscribe((event: any) => {
        this.currentUrl = event.urlAfterRedirects; 
        this.cdr.detectChanges();
    });

  }
  changeSideBar (){
    this.isOpen = !this.isOpen;
  }

  logOut(){
    this.authService.logout().subscribe();
  }
  private checkUserRole(): void {
    var role = this.authService.getRoleFromToken();
    if(role === 'SYSTEM_ADMIN' || role === 'WAREHOUSE_MANAGER') this.isAdminOrManager = true;
    else this.isAdminOrManager = false;

  }
  checkActive(route: string): boolean {
    return this.currentUrl.includes(route);  
  }


}
