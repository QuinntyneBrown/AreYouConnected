import { Inject } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { map } from "rxjs/operators";

export class AuthService {
    constructor(
        @Inject("apiUrl") private readonly _baseUrl:string,
        private readonly _httpClient: HttpClient
    ) { }

    public tryToLogin(options: { username:string, password:string}) {
        return this._httpClient
        .post<any>(`${this._baseUrl}api/users/token`, options)
        .pipe(
            map(x => {

                localStorage.setItem("accessToken", x.accessToken);
                localStorage.setItem("tenantId", x.tenantId);
                localStorage.setItem("userId", x.userId);
                localStorage.setItem("username", x.username);
            })
        );
    }

    public tryToSignOut() {
        localStorage.removeItem("accessToken");
        localStorage.removeItem("tenantId");
        localStorage.removeItem("userId");
        localStorage.removeItem("username");
    }
}