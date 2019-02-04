import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { CoreModule } from './core/core.module';
import { LoginModule } from './login/login.module';
import { HomeModule } from './home/home.module';

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
    { provide: "apiUrl", useValue: "https://localhost:44309/" },
    { provide: "connectionManagerUrl", useValue: "https://localhost:44337/" }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }

