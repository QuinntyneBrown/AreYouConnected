import { Injectable, NgZone, Inject } from "@angular/core";
import { HubConnection, HubConnectionBuilder, IHttpConnectionOptions, LogLevel } from "@aspnet/signalr";
import { Subject, BehaviorSubject } from "rxjs";
import { Router } from "@angular/router";

@Injectable()
export class HubClient {
  public events: Subject<any> = new Subject();

  constructor(
      @Inject("connectionManagerUrl") private readonly _baseUrl:string,      
      private readonly _ngZone: NgZone,
      private readonly _router: Router) {
  }

  private _connection: HubConnection;
  private _connect: Promise<any>;

  public connect(): Promise<any> {
    if (this._connect) return this._connect;

    this._connect = new Promise((resolve, reject) => {

      const options: IHttpConnectionOptions = {
        accessTokenFactory: () => localStorage.getItem("accessToken")
      };

      this._connection = this._connection || new HubConnectionBuilder()
        .withUrl(`${this._baseUrl}hubs/connectionManagement`,options)
        .configureLogging(LogLevel.Information)
        .build();

      this._connection.on("showUsersOnLine", (value) => {
        this._ngZone.run(() => this.usersOnline$.next(value));
      });

      this._connection.on("result", (value) => {
        this._ngZone.run(() => this.events.next(value));
      });

      this._connection.on("connectionId", (value) => {
        localStorage.setItem("connectionId",value);
      });

      this._connection.onclose((e) => {             
        this._router.navigateByUrl("/login");        
        this.disconnect();
      });

      this._connection.start()
        .then(() => resolve())
        .catch(() => {
          reject("Failed Connection");            
        });
    });

    return this._connect;
  }

  public usersOnline$: Subject<string> = new Subject();

  public disconnect() {
    if (this._connection) this._connection.stop();
    this._connect = null;
    this._connection = null;
  }  
}
