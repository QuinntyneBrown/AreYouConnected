import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { environment } from '../environments/environment';
import { HttpClientModule } from '@angular/common/http';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { LoginPageComponent } from './login-page.component';
import { HomePageComponent } from './home-page.component';
import { AuthGuard } from './auth-guard';
import { AuthService } from './auth.service';
import { HubClient } from './hub-client';
import { HubClientGuard } from './hub-client-guard';

@NgModule({
  declarations: [
    AppComponent,
    LoginPageComponent,
    HomePageComponent
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    ReactiveFormsModule,
    FormsModule,
    AppRoutingModule,
  ],
  providers: [
    { provide: "apiUrl", useValue: environment.apiUrl },
    { provide: "connectionManagerUrl", useValue: environment.connectionManagerUrl },

    AuthGuard,
    AuthService,
    HubClient,
    HubClientGuard
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }

