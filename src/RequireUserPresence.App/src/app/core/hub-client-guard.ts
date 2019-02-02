import { Injectable } from "@angular/core";
import { CanActivate, Router } from '@angular/router';
import { HubClient } from "./hub-client";
import { AuthService } from "./auth.service";

@Injectable()
export class HubClientGuard implements CanActivate {
  constructor(
      private _hubClient: HubClient,
      private _router: Router
  ) { }

  public canActivate(): Promise<boolean>  {
    return new Promise((resolve, reject) =>
      this._hubClient.connect().then(() => {
        resolve(true);
      }).catch((e) => {
        reject(e);
        localStorage.setItem("accessToken",null);
        return this._router.navigateByUrl("/login");
      })
    );
  }
}
