import { HttpClient, HttpHeaders } from "@angular/common/http";
import { environment } from "../environments/environment";
import { AuthService } from "./features/auth/auth.service";
import { delay, switchMap, tap } from "rxjs";
import { Injectable } from "@angular/core";

@Injectable({
    providedIn: 'root'
})
export class ApiService {
    constructor(private httpClient: HttpClient, private authService: AuthService) {

    }

    post<TResponse>(relativeUrl: string, body: any) {
        return this.authService.getAccessTokenOrRedirectToLogin().pipe(switchMap(accessToken => {
            const headers = new HttpHeaders({
                'Content-Type': 'application/json',
                Authorization: `Bearer ${accessToken}`,
            });
            return this.httpClient.post<TResponse>(ApiService.getApiUrl(relativeUrl), body, { headers: headers });
        }));
    }

    static getApiUrl(relativeUrl: string) {
        let baseUrl = environment.apiBaseUrl;
        if(baseUrl.endsWith('/')) {
            baseUrl = baseUrl.substring(0, baseUrl.length - 1);
        }

        const trimmedRelativeUrl = relativeUrl.startsWith('/')
            ? relativeUrl.substring(1)
            : relativeUrl;

        return `${baseUrl}/${trimmedRelativeUrl}`
    }
}

export function debugLog(text: string) {
    if(environment.isDebugLogEnabled) {
        console.log(text);
    }
}