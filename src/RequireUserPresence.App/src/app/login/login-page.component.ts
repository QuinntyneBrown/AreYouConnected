import { Component } from "@angular/core";
import { Subject } from "rxjs";
import { AuthService } from "../core/auth.service";
import { Router } from "@angular/router";
import { FormControl, FormGroup, Validators } from "@angular/forms";

@Component({
  templateUrl: "./login-page.component.html",
  styleUrls: ["./login-page.component.css"],
  selector: "app-login-page"
})
export class LoginPageComponent { 
  constructor(
    private readonly _authService: AuthService,
    private readonly _router: Router
    ) { }

  public tryToLogin() {    
    this._authService.tryToLogin({
      username: this.form.value.username,
      password:this.form.value.password
    })
      .subscribe(() => this._router.navigateByUrl("/"));
  }
  
  public onDestroy: Subject<void> = new Subject<void>();

  ngOnDestroy() {
    this.onDestroy.next();	
  }

  public username:string;

  public form = new FormGroup({
    username: new FormControl(this.username, [Validators.required])
  });  
}
