import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { HomePageComponent } from './home-page.component';
import { LoginPageComponent } from './login-page.component';
import { HubClientGuard } from './hub-client-guard';
import { AuthGuard } from './auth-guard';

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
