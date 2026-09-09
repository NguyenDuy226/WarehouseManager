import { NgModule, provideBrowserGlobalErrorListeners } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { AppRoutingModule } from './app-routing-module';
import { App } from './app';
import { SideBar } from './Components/side-bar/side-bar';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { AuthInterceptor } from './Services/Auth Service/auth-service';

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
