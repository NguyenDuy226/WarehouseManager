import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from './Guards/auth-guard';
import { RoleGuard } from './Guards/role-guard';
import { SideBar } from './Components/side-bar/side-bar';

const routes: Routes = [
  {
    path: '',
    component: SideBar,
    canActivate: [AuthGuard],
    children: [
      {
        path: 'manager-dashboard',
        canActivate: [RoleGuard],
        data: {
          expectedRoles: ['SYSTEM_ADMIN', 'WAREHOUSE_MANAGER']
        },
        loadComponent: () => import('./Components/manager-dashboard/manager-dashboard').then(m => m.ManagerDashboard)
      },
      {
        path: 'manager-dashboard/page/:page',
        canActivate: [RoleGuard],
        data: {
          expectedRoles: ['SYSTEM_ADMIN', 'WAREHOUSE_MANAGER']
        },
        loadComponent: () => import('./Components/manager-dashboard/manager-dashboard').then(m => m.ManagerDashboard)
      },
      {
        path: 'dashboard/page/:page', 
        canActivate: [RoleGuard], 
        data: { 
          expectedRoles: ['SYSTEM_ADMIN', 'WAREHOUSE_MANAGER', 'WAREHOUSE_CLERK', 'APPROVER', 'REQUESTER', 'AUDITOR'] 
        },
        loadComponent: () => import('./Components/dash-board/dash-board').then(m => m.DashBoard) 
      },
      {
        path: 'warehouse/:id',
        loadComponent: () => import('./Components/warehouse-detail/warehouse-detail').then(m=> m.WarehouseDetail)
      },
      {
        path: 'waiting-permission',
        loadComponent: () => import('./Components/waiting-permission/waiting-permission').then(m => m.WaitingPermission)
      }
    ]
  },
  {
    path: 'login',
    loadComponent: () => import('./Components/login/login').then(m => m.Login)
  },
  {
    path: 'register',
    loadComponent: () => import('./Components/register/register').then(m => m.Register)
  },
  {
    path: 'change-password',
    loadComponent: () => import('./Components/change-password/change-password').then(m => m.ChangePassword)
  },
  {
    path: '**',
    redirectTo: 'dashboard'    
  },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }