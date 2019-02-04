import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { CoreModule } from './core/core.module';
import { LoginModule } from './login/login.module';
import { HomeModule } from './home/home.module';
import { environment } from '../environments/environment';

@NgModule({
  declarations: [
    AppComponent,
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    CoreModule,
    HomeModule,
    LoginModule
  ],
  providers: [
    { provide: "apiUrl", useValue: environment.apiUrl },
    { provide: "connectionManagerUrl", useValue: environment.connectionManagerUrl }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }

