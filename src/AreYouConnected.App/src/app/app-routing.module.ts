import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { HomePageComponent } from './home/home-page.component';
import { LoginPageComponent } from './login/login-page.component';
import { AuthGuard } from './core/auth-guard';
import { HubClientGuard } from './core/hub-client-guard';

const routes: Routes = [
  {
    path:"",
    component: HomePageComponent,
    canActivate:[AuthGuard, HubClientGuard]
  },
  {
    path:"login",
    component: LoginPageComponent
  }  
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
