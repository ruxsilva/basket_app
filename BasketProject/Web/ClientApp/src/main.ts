import { bootstrapApplication } from '@angular/platform-browser';
import {provideRouter, Routes} from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { importProvidersFrom } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { AppComponent } from './app/app.component';
import { LoginComponent } from './app/components/login/login.component';
import { RegisterComponent } from './app/components/register/register.component';
import { BasketFormComponent } from './app/components/basket-form/basket-form.component';
import { AuthInterceptor } from './app/auth/auth.interceptor';
import { AuthGuard } from './app/auth/auth.guard';
import {BasketHistoryComponent} from './app/components/basket-history/basket-history.component';

// Define routes with explicit pathMatch values of "full" or "prefix"
const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' as const },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  {
    path: 'basket',
    component: BasketFormComponent,
    canActivate: [AuthGuard]
  },
  {
    path: 'basket-history',
    component: BasketHistoryComponent,
    canActivate: [AuthGuard] // Assuming you have an auth guard
  },
  { path: '**', redirectTo: '/login', pathMatch: 'full' as const },

];

bootstrapApplication(AppComponent, {
  providers: [
    provideRouter(routes),
    provideHttpClient(withInterceptors([AuthInterceptor])),
    importProvidersFrom(FormsModule, ReactiveFormsModule)
  ]
}).catch(err => console.error(err));
