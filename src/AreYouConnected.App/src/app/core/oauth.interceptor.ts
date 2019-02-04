import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { HttpEvent, HttpInterceptor, HttpRequest, HttpHandler } from "@angular/common/http";

@Injectable()
export class OAuthInterceptor implements HttpInterceptor {
  intercept(httpRequest: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    
    if (localStorage.getItem("accessToken") == "null" || localStorage.getItem("accessToken") == null)
        return next.handle(httpRequest.clone({ }));

      return next.handle(httpRequest.clone({
          headers: httpRequest.headers.set('Authorization', `Bearer ${localStorage.getItem("accessToken")}`)
      }));
    }
}
