import { NgModule, provideBrowserGlobalErrorListeners } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing-module';
import { App } from './app';
import { SideBar } from './Components/side-bar/side-bar';
import { AuthInterceptor, AuthService } from './Services/auth-service';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { AddUserToWarehouse } from './Components/dash-board/add-user-to-warehouse/add-user-to-warehouse';

@NgModule({
  declarations: [App],
  imports: [BrowserModule, AppRoutingModule, SideBar],
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideHttpClient(withInterceptors([AuthInterceptor])),
  ],
  bootstrap: [App],
})
export class AppModule {}
