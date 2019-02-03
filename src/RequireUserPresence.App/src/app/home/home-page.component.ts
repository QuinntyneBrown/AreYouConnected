import { Component, Inject } from "@angular/core";
import { Subject, Observable } from "rxjs";
import { HubClient } from "../core/hub-client";
import { HttpClient } from "@angular/common/http";
import { AuthService } from "../core/auth.service";
import { Router } from "@angular/router";

@Component({
  templateUrl: "./home-page.component.html",
  styleUrls: ["./home-page.component.css"],
  selector: "app-home-page"
})
export class HomePageComponent { 
  constructor(
    @Inject("apiUrl") private readonly _apiUrl:string,
    private readonly _authService: AuthService,
    private readonly _hubClient: HubClient,
    private readonly _httpClient: HttpClient,
    private readonly _router: Router
  ) {

  }

  ngOnInit() {    
    this.pong$ = this._hubClient.events.asObservable();

    this._httpClient.post(`${this._apiUrl}api/ping`,null,{ headers: {
      "Authorization":`Bearer ${localStorage.getItem("accessToken")}`,
      "ConnectionId": localStorage.getItem("connectionId")
    } }).subscribe();    
  }

  public get usersOnline$() { return this._hubClient.usersOnline$; }

  public pong$:Observable<string>;
  
  public onDestroy: Subject<void> = new Subject<void>();

  ngOnDestroy() {
    this.onDestroy.next();	
  }

  public get username() { return localStorage.getItem("username"); }

  tryToSignOut() {
    this._authService.tryToSignOut();
    this._hubClient.disconnect();
    this._router.navigateByUrl("/login");
  }
}
