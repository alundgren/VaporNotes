

import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, map  } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { getApiUrl } from '../../common';

@Injectable({
    providedIn: 'root'
})
export class AuthService {
    constructor(private httpClient: HttpClient) {
        const storedAuthRaw = sessionStorage.getItem(SessionStorageTokenKey) ?? localStorage.getItem(LocalStorageTokenKey);
        const auth : DropboxAuthData = storedAuthRaw ? JSON.parse(storedAuthRaw) : null;
        this.authState = new BehaviorSubject<{ isAuthenticated: boolean, accessToken ?: string }>(auth
            ? { isAuthenticated: true, accessToken: auth.accessToken }
            : { isAuthenticated: false });
    }

    private readonly authState:  BehaviorSubject<{ isAuthenticated: boolean, accessToken ?: string }>;

    isAuthenticated() {
        return this.authState.value.isAuthenticated;
    }

    accessToken() {
        return this.authState.value.accessToken;
    }

    getLocalLoginUrl() {
        return inject(Router).createUrlTree(['/login']);
    }

    beginDropboxAuthorization() {
        return this.httpClient.post<{ loginUrl: string }> (getApiUrl('api/begin-authorize'), {}).pipe(map(x => x.loginUrl));
    }

    completeDropboxAuthorization(code: string) {
        return this.httpClient.post<DropboxAuthData> (getApiUrl('api/complete-authorize'), { code }).pipe(map(x => {
            sessionStorage.setItem(SessionStorageTokenKey, JSON.stringify(x));
            localStorage.setItem(LocalStorageTokenKey, JSON.stringify(x));
            this.authState.next({ isAuthenticated: true, accessToken: x.accessToken });
            return true;
        }))
    }
/*
    login(email: string, password: string, persistRefreshToken: boolean) {
        return this.apiService.post<AuthData>('v1/identity/login', { email, password }, {
                handleError: err => {
                    if(err.status === 401) {
                        (err as any).message = 'Invalid email or password.';
                        return throwError(() => err);
                    } else {
                        return throwError(() => err);
                    }
                }
        })
        .pipe(map(x => {
            sessionStorage.setItem(AccessTokenKey, JSON.stringify(x));
            if(persistRefreshToken) {
                localStorage.setItem(RefreshTokenKey, JSON.stringify(x));
            }
            this.authState.next({ isAuthenticated: true, accessToken: x.accessToken });
            return true;
        }));
    }*/
}

const LocalStorageTokenKey = 'vapornotes_dropbox_access_local_2024032401'
const SessionStorageTokenKey = 'vapornotes_dropbox_access_refresh_2024032401';

interface DropboxAuthData {
    expiresAt: Date,
    accessToken: string,
    refreshToken: string
}