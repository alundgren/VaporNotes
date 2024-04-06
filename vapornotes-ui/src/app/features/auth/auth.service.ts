import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, map } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { ApiService } from '../../api.service';

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

    private readonly authState: BehaviorSubject<DropboxAuthData | null>;

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
            } else if(!a) {
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

    getLocalLoginUrl() {
        return inject(Router).createUrlTree(['/login']);
    }

    beginDropboxAuthorization() {
        return this.httpClient.post<{ loginUrl: string }>(ApiService.getApiUrl('api/begin-authorize'), {}).pipe(map(x => x.loginUrl));
    }

    completeDropboxAuthorization(code: string) {
        return this.httpClient.post<DropboxAuthData>(ApiService.getApiUrl('api/complete-authorize'), { code }).pipe(map(x => {
            sessionStorage.setItem(SessionStorageTokenKey, JSON.stringify(x));
            localStorage.setItem(LocalStorageTokenKey, JSON.stringify(x));
            this.authState.next(x);
            return true;
        }))
    }
}

const LocalStorageTokenKey = 'vapornotes_dropbox_access_local_2024032401'
const SessionStorageTokenKey = 'vapornotes_dropbox_access_refresh_2024032401';

interface DropboxAuthData {
    expiresAtEpoch: number,
    accessToken: string,
    refreshToken: string
}