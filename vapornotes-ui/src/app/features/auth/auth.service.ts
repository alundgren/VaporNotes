import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, firstValueFrom, map, of } from 'rxjs';
import { HttpClient, HttpHeaders, HttpRequest } from '@angular/common/http';
import { ApiService, debugLog } from '../../api.service';

@Injectable({
    providedIn: 'root'
})
export class AuthService {
    constructor(private httpClient: HttpClient, private router: Router) {
        const storedAuthRaw = sessionStorage.getItem(SessionStorageTokenKey) ?? localStorage.getItem(LocalStorageTokenKey);
        const auth: DropboxAuthData = storedAuthRaw ? JSON.parse(storedAuthRaw) : null;
        this.authState = new BehaviorSubject<DropboxAuthData | null>(auth
            ? auth
            : null);
    }

    public readonly authState: BehaviorSubject<DropboxAuthData | null>;

    public isStateAuthenticated(state: DropboxAuthData | null) {
        return (state && state.expiresAtEpoch > Date.now());
    }

    isAuthenticated() {
        return !!this.authState.value;
    }

    expireAccessToken() {
        let a = this.authState.value;
        if(!a) {
            return;
        }
        this.authState.next({...a, expiresAtEpoch: Date.now() - 5000 })
    }

    getAccessTokenOrRedirectToLogin() {
        let a = this.authState.value;
        return new Observable<string>(x => {
            if(a && a.expiresAtEpoch > Date.now()) {
                x.next(a.accessToken);
                x.complete();
            } else if(!a || a.refreshToken === null) {
                x.complete();
                this.router.navigateByUrl(this.getLocalLoginUrl());
            } else {
                this.httpClient.post<DropboxAuthData>(ApiService.getApiUrl('api/refresh-authorize'), { refreshToken: a.refreshToken }).subscribe(y => {
                    this.authState.next(y);
                    x.next(y.accessToken);
                    x.complete();
                })
            }
        })
    }

    public async authenticateWithGoogleIdToken(googleIdToken: string) {
        const url = ApiService.getApiUrl('api/authenticate');
        const request = { idToken: googleIdToken };
        return this.httpClient.post<{ accessToken: string, expirationDate: Date }>(url, request).pipe(map(x => {
            const auth : DropboxAuthData = {
                expiresAtEpoch: x.expirationDate.valueOf(),
                accessToken: x.accessToken,
                refreshToken: null
            }
            sessionStorage.setItem(SessionStorageTokenKey, JSON.stringify(auth));
            localStorage.setItem(LocalStorageTokenKey, JSON.stringify(auth));
            this.authState.next(auth);
        }));
    }

    getLocalLoginUrl() {
        return this.router.createUrlTree(['/login']);
    }

    logout() {
        sessionStorage.removeItem(SessionStorageTokenKey);
        localStorage.removeItem(LocalStorageTokenKey);
        this.authState.next(null);
        return this.router.navigateByUrl(this.getLocalLoginUrl());
    }
}

const LocalStorageTokenKey = 'vapornotes_access_local_20240512.01'
const SessionStorageTokenKey = 'vapornotes_access_refresh_20240512.01';

interface DropboxAuthData {
    expiresAtEpoch: number,
    accessToken: string,
    refreshToken : string | null
}