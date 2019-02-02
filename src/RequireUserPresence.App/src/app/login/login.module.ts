import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { LoginPageComponent } from "./login-page.component";
import { CoreModule } from "../core/core.module";
import { RouterModule } from "@angular/router";
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

@NgModule({
    declarations:[
        LoginPageComponent
    ],    
    imports: [
        CommonModule,
        CoreModule,
        FormsModule,
        ReactiveFormsModule,
        RouterModule
    ]
})
export class LoginModule {

}