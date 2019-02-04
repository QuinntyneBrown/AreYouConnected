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
                localStorage.setItem("username", x.username);
            })
        );
    }

    public tryToSignOut() {
        localStorage.clear();
    }
}