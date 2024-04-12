import { Injectable } from "@angular/core";
import { BehaviorSubject, Observable } from "rxjs";

@Injectable({
    providedIn: "root",
  })
  export class LoadingService {
    constructor() {
        this.loadingSubject = new BehaviorSubject<boolean>(false);
        this.loading$ = this.loadingSubject.asObservable();
    }

    private loadingSubject : BehaviorSubject<boolean>;

    loading$ : Observable<boolean>

    loadingOn() {
      this.loadingSubject.next(true);
    }

    loadingOff() {
      this.loadingSubject.next(false);
    }
  }