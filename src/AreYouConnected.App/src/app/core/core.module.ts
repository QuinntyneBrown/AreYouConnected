import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { AuthGuard } from "./auth-guard";
import { AuthService } from "./auth.service";
import { HubClient } from "./hub-client";
import { HubClientGuard } from "./hub-client-guard";
import { HttpClientModule, HTTP_INTERCEPTORS } from "@angular/common/http";

@NgModule({
    imports: [
        CommonModule,
        HttpClientModule
    ],
    providers:[
        AuthGuard,
        AuthService,
        HubClient,
        HubClientGuard
    ]
})
export class CoreModule { }