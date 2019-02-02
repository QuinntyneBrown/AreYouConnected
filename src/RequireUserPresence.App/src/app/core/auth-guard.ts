import {Injectable} from "@angular/core";
import {CanActivate,ActivatedRouteSnapshot,RouterStateSnapshot, Router, UrlTree} from '@angular/router';
import {Observable, of} from "rxjs";
import { AuthService } from "./auth.service";

@Injectable()
export class AuthGuard implements CanActivate {
    constructor(                
        private readonly _router: Router
    ) { }

    public canActivate(
        next: ActivatedRouteSnapshot,
        state: RouterStateSnapshot
    ): Observable<boolean> {
        
        if(localStorage.getItem("accessToken"))
            return of(true);

        this._router.navigateByUrl("/login");               

        return of(false);
    }
}
